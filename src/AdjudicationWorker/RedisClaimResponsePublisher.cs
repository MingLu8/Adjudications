using SharedContracts;
using StackExchange.Redis;
using System.Text.Json;

namespace AdjudicationWorker;

public class RedisClaimResponsePublisher
    (
        IConnectionMultiplexer redis,
        RedisSettings settings,
        ILogger<RedisClaimResponsePublisher> logger
    ) : IClaimResponsePublisher
{
    public async Task PublishResponseAsync(ClaimResponse response, CancellationToken token)
    {
        var pub = redis.GetSubscriber();
        var json = JsonSerializer.Serialize(response);
        var channel = RedisChannel.Literal(settings.ResponseChannel);
        await pub.PublishAsync(channel, json);
        logger.LogInformation("Published response Id={Id}", response.TransactionId);
    }
}
