using AdjudicationApi.Abstractions;
using AdjudicationApi.ConfigurationSettings;
using SharedContracts;
using StackExchange.Redis;

namespace AdjudicationApi.Infrastructures
{
    public class RedisClaimProducer(
       IConnectionMultiplexer redis,
       RedisSettings settings,
       ILogger<RedisClaimProducer> logger) : IClaimQueue
    {
        public async Task AddAsync(ClaimRequest claim, CancellationToken token)
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