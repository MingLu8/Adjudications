using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace DeadClaimCleaner
{
    public class DeadClaimCleaner
    {
        private readonly IDatabase _db;
        private readonly ILogger _logger;
        private readonly RedisSettings _redisSettings;

        public DeadClaimCleaner(
            RedisSettings redisSettings,
            IConnectionMultiplexer redis,
            ILoggerFactory loggerFactory)
        {
            _db = redis.GetDatabase();
            _logger = loggerFactory.CreateLogger<DeadClaimCleaner>();
            _redisSettings = redisSettings;
        }

        [Function("CleanupDeadClaims")]
        public async Task Run([TimerTrigger("%TimerSchedule%")] TimerInfo myTimer)
        {
            _logger.LogInformation($"[{_redisSettings.ConsumerName}] started.");

            var autoClaimResult = await _db.StreamAutoClaimAsync(
                _redisSettings.StreamName,
                _redisSettings.ConsumerGroup,
                _redisSettings.ConsumerName,
                minIdleTimeInMs: _redisSettings.DeadClaimAgeInSeconds * 1000,
                startAtId: "0-0");

            if (autoClaimResult.ClaimedEntries.Length > 0)
            {
                var idsToDelete = autoClaimResult.ClaimedEntries.Select(e => e.Id).ToArray();
                await _db.StreamDeleteAsync(_redisSettings.StreamName, idsToDelete);
                _logger.LogInformation($"[{_redisSettings.ConsumerName}] Permanently deleted {idsToDelete.Length} dead claims.");
            }
            else
            {
                _logger.LogInformation($"[{_redisSettings.ConsumerName}] no dead claims found.");
            }
            _logger.LogInformation($"[{_redisSettings.ConsumerName}] completed.");
        }
    }
}