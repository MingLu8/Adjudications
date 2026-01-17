using SharedContracts;

namespace ApiGateway.Abstractions
{
    public interface IResponseMap
    {
        TaskCompletionSource<ClaimResponse> Create(string transactionId);
        bool TryResolve(string transactionId, ClaimResponse response);
        void Remove(string transactionId);
    }

}
