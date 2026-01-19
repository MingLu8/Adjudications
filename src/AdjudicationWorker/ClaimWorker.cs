using System.Diagnostics;
using System.Text.Json;
using Confluent.Kafka;
using SharedContracts;
using StackExchange.Redis;

namespace AdjudicationWorker;

public class ClaimWorker(
    ITaskOrchestrator taskOrchestrator,
    IConsumer<Ignore, string> consumer,
    IProducer<Null, string> dlqProducer,
    IConnectionMultiplexer redis,
    RedisSettings redisSettings,
    KafkaSettings kafkaSettings,
    ActivitySource activitySource,
    ILogger<ClaimWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        consumer.Subscribe(kafkaSettings.ClaimsTopic);
        logger.LogInformation("Adjudication Worker started. Topic={Topic}", kafkaSettings.ClaimsTopic);

        var db = redis.GetDatabase();
        var pub = redis.GetSubscriber();

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                ConsumeResult<Ignore, string>? consumeResult = null;

                try
                {
                    consumeResult = consumer.Consume(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error consuming from Kafka");
                    continue;
                }

                await ProcessMessageAsync(consumeResult, db, pub, stoppingToken);
            }
        }
        finally
        {
            consumer.Close();
        }
    }

    private async Task ProcessMessageAsync(
        ConsumeResult<Ignore, string> consumeResult,
        IDatabase db,
        ISubscriber pub,
        CancellationToken workerToken)
    {
        ClaimRequest? claim = null;

        using var activity = activitySource.StartActivity("ProcessClaim", ActivityKind.Consumer);
        activity?.SetTag("messaging.system", "kafka");
        activity?.SetTag("messaging.destination", kafkaSettings.ClaimsTopic);
        activity?.SetTag("messaging.kafka.partition", consumeResult.Partition.Value);
        activity?.SetTag("messaging.kafka.offset", consumeResult.Offset.Value);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(workerToken);
        cts.CancelAfter(TimeSpan.FromSeconds(kafkaSettings.ClaimTimeoutSeconds));
        var claimToken = cts.Token;

        try
        {
            claim = DeserializeClaim(consumeResult.Message.Value, logger);
            activity?.SetTag("claim.transaction_id", claim.TransactionId);

            logger.LogInformation("Processing Transaction Id={Id}", claim.TransactionId);

            if (await IsStaleClaimAsync(db, claim))
            {
                await HandleStaleClaimAsync(db, pub, claim, consumeResult, workerToken);
                return;
            }

            var response = await ProcessValidClaimAsync(claim, claimToken);
            await PublishResponseWithRetryAsync(pub, response, workerToken);
            consumer.Commit(consumeResult);
        }
        catch (OperationCanceledException) when (!workerToken.IsCancellationRequested)
        {
            logger.LogWarning("Claim timed out. transaction Id={Id}", claim?.TransactionId);
            await SendToDlqAsync(consumeResult.Message.Value, "ClaimTimeout", workerToken);
            consumer.Commit(consumeResult);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing transaction Id={Id}", claim?.TransactionId);

            try
            {
                await SendToDlqAsync(consumeResult.Message.Value, "ProcessingError", workerToken);
                consumer.Commit(consumeResult);
            }
            catch (Exception dlqEx)
            {
                logger.LogError(dlqEx, "Failed to send message to DLQ transaction Id={Id}", claim?.TransactionId);
            }
        }
    }

    // ------------------------------------------------------------
    // STRICT DESERIALIZATION WITH SINGLE-LINE LOGGING
    // ------------------------------------------------------------
    private ClaimRequest DeserializeClaim(string json, ILogger logger)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            logger.LogWarning("Received empty Kafka message payload");
            throw new InvalidDataException("Kafka message payload is empty");
        }

        try
        {
            return JsonSerializer.Deserialize<ClaimRequest>(json)
                   ?? throw new JsonException("Deserialized ClaimRequest is null");
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Failed to deserialize ClaimRequest PayloadLength={Length} Preview={Preview}",
                json.Length,
                json.Length > 200 ? json[..200] + "..." : json);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error deserializing ClaimRequest PayloadLength={Length}", json.Length);
            throw;
        }
    }

    // ------------------------------------------------------------
    // STALE CLAIM CHECK
    // ------------------------------------------------------------
    private async Task<bool> IsStaleClaimAsync(IDatabase db, ClaimRequest claim)
    {
        var key = $"claim_start:{claim.TransactionId}";
        var startValue = await db.StringGetAsync(key);

        if (!startValue.HasValue)
            return false;

        var startTime = (long)startValue;
        var age = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - startTime;

        if (age > redisSettings.MaxProcessingAgeSeconds)
        {
            logger.LogWarning("Stale claim detected Id={Id} AgeSeconds={Age}", claim.TransactionId, age);
            return true;
        }

        return false;
    }

    private async Task HandleStaleClaimAsync(
        IDatabase db,
        ISubscriber pub,
        ClaimRequest claim,
        ConsumeResult<Ignore, string> consumeResult,
        CancellationToken token)
    {
        var key = $"claim_start:{claim.TransactionId}";
        await db.KeyDeleteAsync(key);

        var response = new ClaimResponse
        {
            TransactionId = claim.TransactionId,
            NcpdpResponsePayload = $"REJECTED|{claim.NcpdpPayload}|TIMEOUT",
            Success = true
        };

        await PublishResponseWithRetryAsync(pub, response, token);
        consumer.Commit(consumeResult);
    }

    // ------------------------------------------------------------
    // VALID CLAIM PROCESSING
    // ------------------------------------------------------------
    private async Task<ClaimResponse> ProcessValidClaimAsync(ClaimRequest claim, CancellationToken token)
    {
        using var activity = activitySource.StartActivity("AdjudicateClaim", ActivityKind.Internal);
        activity?.SetTag("claim.transaction_id", claim.TransactionId);

        await taskOrchestrator.ProcessClaimRequestAsync(claim);
        await Task.Delay(50, token);

        return new ClaimResponse
        {
            TransactionId = claim.TransactionId,
            NcpdpResponsePayload = $"PAID|{claim.NcpdpPayload}|APPROVED",
            Success = true
        };
    }

    // ------------------------------------------------------------
    // REDIS PUBLISH WITH RETRY/BACKOFF
    // ------------------------------------------------------------
    private async Task PublishResponseWithRetryAsync(
        ISubscriber pub,
        ClaimResponse response,
        CancellationToken token)
    {
        var json = JsonSerializer.Serialize(response);
        var channel = RedisChannel.Literal(redisSettings.ResponseChannel);

        const int maxAttempts = 5;
        var delay = TimeSpan.FromMilliseconds(100);

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await pub.PublishAsync(channel, json);
                logger.LogInformation("Published response Id={Id} Attempt={Attempt}", response.TransactionId, attempt);
                return;
            }
            catch (Exception ex) when (attempt < maxAttempts && !token.IsCancellationRequested)
            {
                logger.LogWarning(ex, "Redis publish failed Id={Id} Attempt={Attempt} DelayMs={Delay}",
                    response.TransactionId, attempt, delay.TotalMilliseconds);

                await Task.Delay(delay, token);
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2);
            }
        }

        logger.LogError("Redis publish exhausted retries Id={Id}", response.TransactionId);
    }

    // ------------------------------------------------------------
    // KAFKA DLQ SUPPORT
    // ------------------------------------------------------------
    private async Task SendToDlqAsync(string originalPayload, string reason, CancellationToken token)
    {
        var dlqEnvelope = new
        {
            Reason = reason,
            Timestamp = DateTimeOffset.UtcNow,
            Payload = originalPayload
        };

        var json = JsonSerializer.Serialize(dlqEnvelope);

        await dlqProducer.ProduceAsync(
            kafkaSettings.DlqTopic,
            new Message<Null, string> { Value = json },
            token);

        logger.LogWarning("Sent message to DLQ Reason={Reason}", reason);
    }
}
