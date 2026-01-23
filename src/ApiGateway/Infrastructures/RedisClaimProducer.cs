using ApiGateway.Abstractions;
using ApiGateway.ConfigurationSettings;
using SharedContracts;
using StackExchange.Redis;

namespace ApiGateway.Infrastructures
{
    public class RedisClaimProducer(
       IConnectionMultiplexer redis,
       RedisSettings settings,
       ILogger<KafkaClaimProducer> logger) : IClaimProducer
    {
        public async Task ProduceAsync(ClaimRequest claim, CancellationToken token)
        {
            try
            {
                logger.LogInformation($"Queuing claim request: {claim.TransactionId}.");

                var db = redis.GetDatabase();
                await db.StreamAddAsync(settings.RequestTopic,
                [
                    new(nameof(claim.TransactionId), claim.TransactionId),
                    new(nameof(claim.NcpdpPayload), claim.NcpdpPayload),
                    new(nameof(claim.ReceivedAt), claim.ReceivedAt) // Record the exact start time
                ],
                maxLength: settings.StreamLimit,
                useApproximateMaxLength: true);
             
                logger.LogInformation($"Queued claim request: {claim.TransactionId}.");

            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Queue claim request failed: {claim.TransactionId}, error: {ex.Message}.");
                throw;
            }
        }
    }

}