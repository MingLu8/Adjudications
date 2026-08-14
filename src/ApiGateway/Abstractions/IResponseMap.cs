using SharedContracts;

namespace AdjudicationApi.Abstractions;

public interface IResponseMap
{
    TaskCompletionSource<ClaimResponse> Create(string transactionId);
    bool TryResolve(string transactionId, ClaimResponse response);
    void Remove(string transactionId);

    /// <summary>
    /// Evicts entries older than <paramref name="maxAge"/>, cancelling their
    /// TaskCompletionSource so any awaiter unblocks instead of hanging forever.
    /// Returns the number of entries evicted.
    /// </summary>
    int EvictExpired(TimeSpan maxAge);
}