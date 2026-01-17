namespace AdjudicationWorker;

using Confluent.Kafka;
using SharedContracts;
using StackExchange.Redis;
using System.Security.Claims;
using System.Text.Json;

public class ClaimWorker(
    IConsumer<Ignore, string> consumer,
    IConnectionMultiplexer redis,
    RedisSettings redisSettings,
    KafkaSettings kafkaSettings,
    ILogger<ClaimWorker> logger) : BackgroundService
{   
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        consumer.Subscribe(kafkaSettings.ClaimsTopic);
       
        logger.LogInformation("Adjudication Worker Started...");
        ClaimRequest? claim = null;
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var db = redis.GetDatabase();
                    var sub = redis.GetSubscriber();

                    // 1. Consume from Kafka
                    var consumeResult = consumer.Consume(stoppingToken);
                    claim = JsonSerializer.Deserialize<ClaimRequest>(consumeResult.Message.Value);

                    if (claim == null) continue;

                    logger.LogInformation($"Processing Claim: {claim.TransactionId}");
                    var startTimeValue = await db.StringGetAsync($"claim_start:{claim.TransactionId}");

                    if (startTimeValue.HasValue)
                    {
                        var startTime = (long)startTimeValue;
                        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                        if (currentTime - startTime > redisSettings.MaxProcessingAgeSeconds)
                        {
                            logger.LogWarning("Bypassing stale claim {Id}. Age: {Age}s",
                                claim.TransactionId, currentTime - startTime);

                            // Cleanup and move to next message without processing
                            await db.KeyDeleteAsync($"claim_start:{claim.TransactionId}");
                            await sub.PublishAsync(RedisChannel.Literal(redisSettings.ResponseChannel), JsonSerializer.Serialize(new ClaimResponse
                            {
                                TransactionId = claim.TransactionId,
                                NcpdpResponsePayload = $"REJECTED|{claim.NcpdpPayload}|TIMEOUT",
                                Success = true
                            }));
                            consumer.Commit(consumeResult);
                            continue;
                        }
                    }

                    // 2. SIMULATE ADJUDICATION LOGIC (Pricing, DUR, etc.)
                    var adjudicationResult = await ProcessClaimAsync(claim);// await Task.Delay(50); // Simulate 50ms processing time
                    var responsePayload = $"PAID|{claim.NcpdpPayload}|APPROVED";

                    // 3. Create Response
                    var response = new ClaimResponse
                    {
                        TransactionId = claim.TransactionId,
                        NcpdpResponsePayload = responsePayload,
                        Success = true
                    };

                    // 4. Publish to Redis (The Broadcast)
                   
                    await sub.PublishAsync(RedisChannel.Literal(redisSettings.ResponseChannel), JsonSerializer.Serialize(response));
                    consumer.Commit(consumeResult);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error processing individual claim message: {claim?.TransactionId}.");
                    // Optional: consumer.StoreOffset(consumeResult) or handle DLQ
                }
            }
        }
        catch (OperationCanceledException)
        {
            consumer.Close();
            logger.LogWarning($"Operation cancelled for claim message: {claim?.TransactionId}.");
        }
    }

    public async Task<AdjudicationResult> ProcessClaimAsync(ClaimRequest claim)
    {
        // 1. Start Service A and Service B in parallel
        // We don't use 'await' here yet!
        //Task<EligibilityResult> eligibilityTask = _eligibilityClient.CheckAsync(claim);
        //Task<FormularyResult> formularyTask = _formularyClient.GetRulesAsync(claim);

        //try
        //{
        //    // 2. Wait for both to complete
        //    await Task.WhenAll(eligibilityTask, formularyTask);

        //    // 3. Extract results (guaranteed to be finished)
        //    var eligibility = await eligibilityTask;
        //    var formulary = await formularyTask;

        //    // 4. Conditional logic for Service C
        //    if (eligibility.IsCovered && formulary.RequiresPriorAuth)
        //    {
        //        // Call the third service only if needed
        //        var paResult = await _priorAuthClient.RequestAsync(claim, formulary.PaCriteria);
        //        return ProcessFinalDecision(eligibility, formulary, paResult);
        //    }

        //}
        //catch (Exception)
        //{
        //    if(eligibilityTask.IsFaulted) _logger.LogError("Eligibility Service Down");
        //    if(formularyTask.IsFaulted) _logger.LogError("Formulary Service Down");
        //    throw;
        //}

        //return ProcessFinalDecision(eligibility, formulary, null);
        await Task.Delay(50000); // Simulate 50ms processing time
        return new AdjudicationResult();
    }
}