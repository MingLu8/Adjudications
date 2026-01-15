namespace AdjudicationWorker;

using System.Text.Json;
using Confluent.Kafka;
using StackExchange.Redis;
using SharedContracts;

public class Worker : BackgroundService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<Worker> _logger;

    public Worker(IConnectionMultiplexer redis, ILogger<Worker> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = "localhost:9092",
            SaslPassword = "redis123",
            GroupId = "adjudication-worker-group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        consumer.Subscribe("pharmacy-claims");

        _logger.LogInformation("Adjudication Worker Started...");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // 1. Consume from Kafka
                var consumeResult = consumer.Consume(stoppingToken);
                var claimRequest = JsonSerializer.Deserialize<ClaimRequest>(consumeResult.Message.Value);

                if (claimRequest == null) continue;

                _logger.LogInformation($"Processing Claim: {claimRequest.TransactionId}");

                // 2. SIMULATE ADJUDICATION LOGIC (Pricing, DUR, etc.)
                await Task.Delay(50); // Simulate 50ms processing time
                var responsePayload = $"PAID|{claimRequest.NcpdpPayload}|APPROVED";

                // 3. Create Response
                var response = new ClaimResponse
                {
                    TransactionId = claimRequest.TransactionId,
                    NcpdpResponsePayload = responsePayload,
                    Success = true
                };

                // 4. Publish to Redis (The Broadcast)
                var sub = _redis.GetSubscriber();
                await sub.PublishAsync("claims-responses", JsonSerializer.Serialize(response));
            }
        }
        catch (OperationCanceledException)
        {
            consumer.Close();
        }
    }
}