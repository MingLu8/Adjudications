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
            => apiCaller.PostAsync<CoverageRequest, CoverageResponse>(httpClient, "get", request, token);
    }
}
