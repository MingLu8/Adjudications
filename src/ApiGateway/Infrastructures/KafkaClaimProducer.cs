using ApiGateway.Abstractions;
using ApiGateway.ConfigurationSettings;
using Confluent.Kafka;
using SharedContracts;
using StackExchange.Redis;
using System.Text.Json;

namespace ApiGateway.Infrastructures
{
    public class KafkaClaimProducer(
        IConnectionMultiplexer redis,
        IProducer<Null, string> producer,
        KafkaSettings settings,
        ILogger<KafkaClaimProducer> logger) : IClaimProducer
    {       
        public async Task ProduceAsync(ClaimRequest claim, CancellationToken token)
        {
            try
            {
                logger.LogInformation($"Queuing claim request: {claim.TransactionId}.");

                var db = redis.GetDatabase();
                var startTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                // Store the start time; expire the key after 5 minutes (safety margin)
                await db.StringSetAsync($"claim_start:{claim.TransactionId}", startTime, TimeSpan.FromMinutes(5));

                var json = JsonSerializer.Serialize(claim);
                await producer.ProduceAsync(
                    settings.RequestTopic,
                    new Message<Null, string> { Value = json },
                    token);
                logger.LogInformation($"Queued claim request: {claim.TransactionId}.");

            }
            catch(Exception ex)
            {
                logger.LogError($"Queue claim request failed: {claim.TransactionId}.");
                throw;
            }
        }
    }

}