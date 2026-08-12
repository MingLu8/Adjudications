using SharedContracts;

namespace ApiGateway.Abstractions
{

    public interface IClaimQueue 
    { 
        Task AddAsync(ClaimRequest request, CancellationToken token);
    }

}
