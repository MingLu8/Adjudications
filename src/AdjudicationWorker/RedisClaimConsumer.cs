using SharedContracts;
using StackExchange.Redis;

namespace AdjudicationWorker;

public class RedisClaimConsumer
    (
       IConnectionMultiplexer redis,
       RedisSettings settings,
       ILogger<RedisClaimConsumer> logger
    ) : IClaimConsumer
{
    public async Task<ClaimRequest> ConsumeAsync(CancellationToken token)
    {
        var db = redis.GetDatabase();
        await EnsureConsumerGroupExistsAsync(db);

        var messages = await db.StreamReadGroupAsync(
                 settings.StreamName,
                 settings.ConsumerGroup,
                 settings.ConsumerName,
                 ">",
                 count: 1);
        if (messages.Length == 0) throw new NullMessageLengthException();

        var msg = messages[0];
        var transactionId = msg.Values.First(v => v.Name == "TransactionId").Value;
        var startedAtTicks = (long)msg.Values.First(v => v.Name == "StartedAt").Value;

        var ageTicks = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - startedAtTicks;

        if (ageTicks > settings.ClaimTimeoutSeconds)
        {
            await db.StreamAcknowledgeAsync(settings.StreamName, settings.ConsumerGroup, msg.Id);
            throw new StaleMessageException(transactionId, TimeSpan.FromTicks(ageTicks).TotalSeconds);
        }

        var payload = msg.Values.First(v => v.Name == "Payload").Value;
        return new ClaimRequest(transactionId, payload, startedAtTicks);          
    }

    private async Task EnsureConsumerGroupExistsAsync(IDatabase db)
    {
        try
        {
            await db.StreamCreateConsumerGroupAsync(settings.StreamName, settings.ConsumerGroup, "0-0");
        }
        catch (RedisServerException ex) when (ex.Message.Contains("BUSYGROUP"))
        {
            // Consumer group already exists; no action needed
        }
    }
}
