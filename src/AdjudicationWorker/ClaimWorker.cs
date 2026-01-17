namespace AdjudicationWorker;

using System.Text.Json;
using Confluent.Kafka;
using StackExchange.Redis;
using SharedContracts;

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

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // 1. Consume from Kafka
                    var consumeResult = consumer.Consume(stoppingToken);
                    var claimRequest = JsonSerializer.Deserialize<ClaimRequest>(consumeResult.Message.Value);

                    if (claimRequest == null) continue;

                    logger.LogInformation($"Processing Claim: {claimRequest.TransactionId}");

                    // 2. SIMULATE ADJUDICATION LOGIC (Pricing, DUR, etc.)
                    var adjudicationResult = await ProcessClaimAsync(claimRequest);// await Task.Delay(50); // Simulate 50ms processing time
                    var responsePayload = $"PAID|{claimRequest.NcpdpPayload}|APPROVED";

                    // 3. Create Response
                    var response = new ClaimResponse
                    {
                        TransactionId = claimRequest.TransactionId,
                        NcpdpResponsePayload = responsePayload,
                        Success = true
                    };

                    // 4. Publish to Redis (The Broadcast)
                    var sub = redis.GetSubscriber();
                    await sub.PublishAsync(RedisChannel.Literal(redisSettings.ResponseChannel), JsonSerializer.Serialize(response));
                    consumer.Commit(consumeResult);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing individual claim message.");
                    // Optional: consumer.StoreOffset(consumeResult) or handle DLQ
                }
            }
        }
        catch (OperationCanceledException)
        {
            consumer.Close();
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
        await Task.Delay(50); // Simulate 50ms processing time
        return new AdjudicationResult();
    }
}