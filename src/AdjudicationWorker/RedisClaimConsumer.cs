using SharedContracts;
using StackExchange.Redis;

namespace AdjudicationWorker;

public class RedisClaimConsumer : IClaimConsumer
{
    private readonly IConnectionMultiplexer _redis;
    private readonly RedisSettings _settings;
    private readonly ILogger<RedisClaimConsumer> _logger;

    private readonly IDatabase _db;
    private readonly string _stream;
    private readonly string _group;
    private readonly string _consumer;

    public RedisClaimConsumer(
       IConnectionMultiplexer redis,
       RedisSettings settings,
       ILogger<RedisClaimConsumer> logger
    )
    {
        _redis = redis;
        _settings = settings;
        _logger = logger;
        _db = _redis.GetDatabase();
        _stream = _settings.StreamName;
        _group = _settings.ConsumerGroup;
        _consumer = Environment.MachineName + "-" + Guid.NewGuid().ToString("N")[..6];
    }

    public async Task<ClaimRequest?> ConsumeAsync(CancellationToken token)
    {
        StreamEntry? msg = null;
        string transactionId = string.Empty;
        try
        {            
            msg = await GetMessageAsync();
            if(msg == null) return null;
            
            transactionId = await ExtractTransactionIdAsync(msg.Value);
            var startedAtTicks = ExtractStartedAt(msg.Value);

            await EnsureFreshMessageAsync(msg.Value, transactionId, startedAtTicks);

            var payload = ExtractPayload(msg.Value);
            return new ClaimRequest(transactionId, payload, startedAtTicks);
        }    
        catch (TransactionIdIsEmptyException ex)
        {
            _logger.LogError(ex, "Message transaction id is empty exception, event=consume-failed error={ErrorMessage}", ex.Message);
            return null;
        }
        catch (StaleMessageException ex)
        {
            _logger.LogError(ex, $"Stale message exception, Transaction id: {ex.TransactionId}, message age:{ex.AgeSeconds}, event=consume-failed error={ex.Message}");
            return null;
        }
        catch (MessageFieldNotFoundException ex)
        {
            _logger.LogError(ex, $"Message field not found excetpion, Transaction id: {transactionId}, event=consume-failed error={ex.Message}");
            return null;
        }
        catch (PayloadIsEmptyException ex)
        {
            _logger.LogError(ex, $"Message payload is empty excetpion, Transaction id: {transactionId}, event=consume-failed error={ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "event=consume-failed error={ErrorMessage}", ex.Message);
            return null;
        }
        finally
        {
            if(msg != null)
                await AcknowledgeAsync(msg.Value.Id!);
        }
    }

    private async Task AcknowledgeAsync(string messageId)
    {
        await _db.StreamAcknowledgeAsync(_stream, _group, messageId);
    }

    private async Task EnsureFreshMessageAsync(StreamEntry msg, string transactionId, long startedAtTicks)
    {
        var ageTicks = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - startedAtTicks;
        if (ageTicks > _settings.ClaimTimeoutSeconds)
        {
            await _db.StreamAcknowledgeAsync(_stream, _group, msg.Id);
            throw new StaleMessageException(transactionId, TimeSpan.FromTicks(ageTicks).TotalSeconds);
        }
    }

    private async Task<StreamEntry?> GetMessageAsync()
    {
        var messages = await _db.StreamReadGroupAsync(_stream, _group, _consumer, ">", count: 1);
        if (messages.Length == 0) return null;
        var msg = messages[0];
        return msg;
    }

    private static string ExtractPayload(StreamEntry msg)
    {
        var payload = msg.Values.FirstOrDefault(v => v.Name == nameof(ClaimRequest.NcpdpPayload)).Value;
        if (!payload.HasValue) throw new MessageFieldNotFoundException(nameof(ClaimRequest.NcpdpPayload));
        if(string.IsNullOrEmpty(payload))
        {
            throw new PayloadIsEmptyException();
        }
        return payload!;
    }

    private static long ExtractStartedAt(StreamEntry msg)
    {
        var receivedAt = msg.Values.FirstOrDefault(v => v.Name == nameof(ClaimRequest.ReceivedAt)).Value;
        if (!receivedAt.HasValue) throw new MessageFieldNotFoundException(nameof(ClaimRequest.ReceivedAt));
        return (long)receivedAt;
    }

    private async Task<string> ExtractTransactionIdAsync(StreamEntry msg)
    {
        var transactionId = msg.Values.FirstOrDefault(v => v.Name == nameof(ClaimRequest.TransactionId)).Value;
        if (!transactionId.HasValue) throw new MessageFieldNotFoundException(nameof(ClaimRequest.TransactionId));
        if (string.IsNullOrEmpty(transactionId))
        {
            await _db.StreamAcknowledgeAsync(_stream, _group, msg.Id);
            throw new TransactionIdIsEmptyException();
        }

        return transactionId!;
    }

    public async Task EnsureConsumerGroupExistsAsync()
    {       
        try 
        { 
            await _db.StreamCreateConsumerGroupAsync(_stream, _group, "0-0", createStream: true); 
            _logger.LogInformation("stream={Stream} event=group-created group={Group}", _stream, _group);
        } 
        catch (RedisServerException ex) when (ex.Message.Contains("BUSYGROUP")) 
        { 
            _logger.LogInformation("stream={Stream} event=group-exists group={Group}", _stream, _group);
        }
    }
}
