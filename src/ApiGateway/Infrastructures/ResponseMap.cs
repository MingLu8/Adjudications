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
            _map.TryAdd(transactionId, tcs);
            return tcs;
        }

        public bool TryResolve(string transactionId, ClaimResponse response)
        {
            if (_map.TryRemove(transactionId, out var tcs))
            {
                logger.LogInformation("Connection found for TransactionId: {TransactionId}", transactionId);
                if(tcs.TrySetResult(response))
                    logger.LogInformation("Forworded response for TransactionId: {TransactionId}", transactionId);
                else
                    logger.LogWarning("Failed to forward response for TransactionId: {TransactionId}", transactionId);
                return true;
            }
            else
                logger.LogWarning("Connection not found for TransactionId: {TransactionId}", transactionId);
            return false;
        }

        public void Remove(string transactionId) => _map.TryRemove(transactionId, out _);
    }


}