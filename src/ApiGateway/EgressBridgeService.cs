namespace ApiGateway;

using System.Collections.Concurrent;
using System.Text.Json;
using StackExchange.Redis;
using SharedContracts;

public class EgressBridgeService : BackgroundService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<ClaimResponse>> _map;

    public EgressBridgeService(IConnectionMultiplexer redis, ConcurrentDictionary<string, TaskCompletionSource<ClaimResponse>> map)
    {
        _redis = redis;
        _map = map;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var sub = _redis.GetSubscriber();

        // Subscribe to the "Global Broadcast" channel
        await sub.SubscribeAsync("claims-responses", (channel, message) =>
        {
            var response = JsonSerializer.Deserialize<ClaimResponse>((string)message!);

            // Check: "Is the waiting pharmacy connected to ME?"
            if (response != null && _map.TryRemove(response.TransactionId, out var tcs))
            {
                // YES! Wake up the API thread.
                tcs.SetResult(response);
            }
            // If not found, it means another Gateway pod owns this connection. We ignore it.
        });

        // Keep the service alive
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
