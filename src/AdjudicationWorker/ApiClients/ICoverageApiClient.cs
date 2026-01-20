using SharedContracts;

namespace AdjudicationWorker.ApiClients
{
    public interface ICoverageApiClient
    {
        Task<CoverageResponse> GetCoverageAsync(CoverageRequest request, CancellationToken token);
    }
}
