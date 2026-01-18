namespace AdjudicationWorker.ApiClients
{
    public class PricingApiClient(
        HttpClient httpClient,
        PricingApiSettings config,
        IApiCaller apiCaller) : IPricingApiClient
    {       
        public Task<PricingResponse> GetPricingAsync(
            PricingRequest request,
            CancellationToken token)
            => apiCaller.PostAsync<PricingRequest, PricingResponse>(httpClient, "get", request, token);
    }
}
