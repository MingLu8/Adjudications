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
        {
            return apiCaller.PostAsync<PricingRequest, PricingResponse>(httpClient, config.RouteTemplate.Replace("{endpoint}", "pricings"), request, token);
        }
    }
}
