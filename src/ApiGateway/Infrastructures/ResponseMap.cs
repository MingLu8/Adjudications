using ApiGateway.Abstractions;
using SharedContracts;
using System.Collections.Concurrent;

namespace ApiGateway.Infrastructures
{
    public class ResponseMap : IResponseMap
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
                tcs.TrySetResult(response);
                return true;
            }
            return false;
        }

        public void Remove(string transactionId) => _map.TryRemove(transactionId, out _);
    }


}