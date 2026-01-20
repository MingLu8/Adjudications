using SharedContracts;

namespace AdjudicationWorker.ApiClients
{
    public class CoverageApiClient(
        HttpClient httpClient,
        CoverageApiSettings config,
        IApiCaller apiCaller) : ICoverageApiClient
    {       

        public Task<CoverageResponse> GetCoverageAsync(
            CoverageRequest request,
            CancellationToken token)
        {
            return apiCaller.PostAsync<CoverageRequest, CoverageResponse>(httpClient, config.RouteTemplate.Replace("{endpoint}", "coverages"), request, token);
        }
    }
}
