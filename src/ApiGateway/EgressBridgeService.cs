namespace ApiGateway;

using ApiGateway.Abstractions;
using ApiGateway.ConfigurationSettings;
using ApiGateway.Infrastructures;
using SharedContracts;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Text.Json;

/// <summary>
/// Listens to a Redis backplane and resolves local TaskCompletionSources 
/// when a response matching a local TransactionId is received.
/// </summary>
public class EgressBridgeService(
    IConnectionMultiplexer redis,
    IResponseMap map,
    RedisSettings redisSettings,
    ILogger<EgressBridgeService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var sub = redis.GetSubscriber();
        var channel = RedisChannel.Literal(redisSettings.ResponseChannel);

        logger.LogInformation("Subscribing to Redis channel: {Channel}", channel);

        // Subscribe to the backplane
        await sub.SubscribeAsync(channel, HandleMessage);

        try
        {
            // Wait until the service is told to stop
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("StoppingToken triggered; shutting down EgressBridgeService.");
        }
        finally
        {
            logger.LogInformation("Unsubscribing from Redis channel: {Channel}", channel);
            await sub.UnsubscribeAsync(channel);
        }
    }

    private void HandleMessage(RedisChannel channel, RedisValue message)
    {
        try
        {
            if (message.IsNullOrEmpty) return;

            var response = JsonSerializer.Deserialize<ClaimResponse>((string)message!);
            if (response?.TransactionId == null)
            {
                logger.LogWarning("Received invalid payload on channel {Channel}", channel);
                return;
            }

            // Check if this specific Gateway instance is waiting for this TransactionId
            if (map.TryResolve(response.TransactionId, response))
            {
                logger.LogDebug("Local match found! Resolving TransactionId: {Id}", response.TransactionId);
            }
            else
            {
                // This is expected behavior in a multi-pod environment
                logger.LogTrace("TransactionId {Id} not owned by this instance.", response.TransactionId);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process Redis message on channel {Channel}", channel);
        }
    }
}