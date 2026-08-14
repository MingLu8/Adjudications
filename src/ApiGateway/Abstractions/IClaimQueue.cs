using SharedContracts;

namespace AdjudicationApi.Abstractions
{

    public interface IClaimQueue 
    { 
        Task AddAsync(ClaimRequest request, CancellationToken token);
    }

}
