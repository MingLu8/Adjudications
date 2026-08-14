using AdjudicationApi.Abstractions;
using SharedContracts;
using System.Collections.Concurrent;

namespace AdjudicationApi.Infrastructures;

// =====================================================================
// IResponseMap.cs
// =====================================================================



// =====================================================================
// ResponseMap.cs
// =====================================================================
public class ResponseMap(ILogger<ResponseMap> logger) : IResponseMap
{
    private sealed record Entry(TaskCompletionSource<ClaimResponse> Tcs, DateTimeOffset CreatedAt);

    private readonly ConcurrentDictionary<string, Entry> _map = new();

    public TaskCompletionSource<ClaimResponse> Create(string transactionId)
    {
        var tcs = new TaskCompletionSource<ClaimResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
        var entry = new Entry(tcs, DateTimeOffset.UtcNow);

        if (!_map.TryAdd(transactionId, entry))
        {
            logger.LogWarning(
                "Duplicate pending response entry for TransactionId: {TransactionId}",
                transactionId);
            throw new DuplicateTransactionException(transactionId);
        }

        logger.LogDebug("Registered pending response for TransactionId: {TransactionId}", transactionId);
        return tcs;
    }

    public bool TryResolve(string transactionId, ClaimResponse response)
    {
        if (!_map.TryRemove(transactionId, out var entry))
        {
            logger.LogWarning("No pending response found for TransactionId: {TransactionId}", transactionId);
            return false;
        }

        logger.LogDebug("Pending response found for TransactionId: {TransactionId}", transactionId);

        if (entry.Tcs.TrySetResult(response))
            logger.LogDebug("Forwarded response for TransactionId: {TransactionId}", transactionId);
        else
            logger.LogWarning("Failed to forward response for TransactionId: {TransactionId}", transactionId);

        return true;
    }

    public void Remove(string transactionId)
    {
        _map.TryRemove(transactionId, out _);
        logger.LogDebug("Removed pending response entry for TransactionId: {TransactionId}", transactionId);
    }

    public int EvictExpired(TimeSpan maxAge)
    {
        var cutoff = DateTimeOffset.UtcNow - maxAge;
        var evictedCount = 0;

        // Snapshot keys first — avoids mutating the dictionary while enumerating it.
        foreach (var key in _map.Keys)
        {
            if (!_map.TryGetValue(key, out var entry) || entry.CreatedAt > cutoff)
                continue;

            if (!_map.TryRemove(key, out var removedEntry))
                continue; // lost the race with TryResolve/Remove — fine, it completed normally

            var wasStillPending = removedEntry.Tcs.TrySetCanceled();
            evictedCount++;

            logger.LogWarning(
                "Pending response entry for TransactionId: {TransactionId} exceeded max age of {MaxAge} and was evicted. StillPending: {StillPending}",
                key, maxAge, wasStillPending);
        }

        return evictedCount;
    }
}


// =====================================================================
// ResponseMapSweeper.cs
// =====================================================================
public class ClaimProcessorSettings
{
    /// <summary>How often the sweep runs.</summary>
    public int SweepInterval { get; set; } = 15;

    /// <summary>
    /// Max age an entry can reach before being evicted. Should comfortably
    /// exceed ClaimProcessorSettings.TimeoutSeconds — this is a backstop,
    /// not the primary timeout mechanism.
    /// </summary>
    public int MaxEntryAge { get; set; } = 60;
}

public class ResponseMapSweeper(
    IResponseMap responseMap,
    ClaimProcessorSettings settings,
    ILogger<ResponseMapSweeper> logger) : BackgroundService
{

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "ResponseMapSweeper started. Interval: {Interval}, MaxEntryAge: {MaxAge}",
            settings.SweepInterval, settings.MaxEntryAge);

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(settings.SweepInterval));

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    var evicted = responseMap.EvictExpired(TimeSpan.FromSeconds(settings.MaxEntryAge));

                    if (evicted > 0)
                        logger.LogWarning("ResponseMapSweeper evicted {Count} stale entries.", evicted);
                }
                catch (Exception ex)
                {
                    // Never let a sweep failure kill the background service —
                    // log and keep sweeping on the next tick.
                    logger.LogError(ex, "ResponseMapSweeper encountered an error during eviction sweep.");
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Expected on shutdown.
        }

        logger.LogInformation("ResponseMapSweeper stopped.");
    }
}


// =====================================================================
// Registration (Program.cs / Startup)
// =====================================================================
// services.AddSingleton<IResponseMap, ResponseMap>();
//
// services.Configure<ResponseMapSweeperOptions>(o =>
// {
//     o.SweepInterval = TimeSpan.FromSeconds(15);
//     o.MaxEntryAge = TimeSpan.FromSeconds(settings.TimeoutSeconds * 3); // stay well above ProcessAsync's own timeout
// });
// services.AddHostedService<ResponseMapSweeper>();

