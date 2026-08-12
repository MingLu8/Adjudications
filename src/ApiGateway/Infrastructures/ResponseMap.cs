using ApiGateway.Abstractions;
using SharedContracts;
using System.Collections.Concurrent;

namespace ApiGateway.Infrastructures
{
    public class ResponseMap(ILogger<ResponseMap> logger) : IResponseMap
    {
        private readonly ConcurrentDictionary<string, TaskCompletionSource<ClaimResponse>> _map = new();

        public TaskCompletionSource<ClaimResponse> Create(string transactionId)
        {
            var tcs = new TaskCompletionSource<ClaimResponse>(TaskCreationOptions.RunContinuationsAsynchronously);

            if (!_map.TryAdd(transactionId, tcs))
            {
                logger.LogWarning("Duplicate pending response entry for TransactionId: {TransactionId}", transactionId);
                throw new DuplicateTransactionException(transactionId);
            }

            logger.LogDebug("Registered pending response for TransactionId: {TransactionId}", transactionId);
            return tcs;
        }

        public bool TryResolve(string transactionId, ClaimResponse response)
        {
            if (!_map.TryRemove(transactionId, out var tcs))
            {
                logger.LogWarning("Connection not found for TransactionId: {TransactionId}", transactionId);
                return false;
            }

            logger.LogDebug("Connection found for TransactionId: {TransactionId}", transactionId);

            if (tcs.TrySetResult(response))
                logger.LogDebug("Forwarded response for TransactionId: {TransactionId}", transactionId);
            else
                logger.LogWarning("Failed to forward response for TransactionId: {TransactionId}", transactionId);

            return true;
        }

        public void Remove(string transactionId)
        {
            _map.TryRemove(transactionId, out _);
            logger.LogInformation("Removed connection map for TransactionId: {TransactionId}", transactionId);
        }
    }
}