using SharedContracts;

namespace AdjudicationApi.Infrastructures
{
    public interface IClaimIngestionService
    {
        Task<ClaimResponse> ProcessAsync(ClaimRequest claim, CancellationToken userToken);
    }
}