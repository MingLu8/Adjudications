
using SharedContracts;

namespace AdjudicationWorker.ApiClients
{
    public interface IPricingApiClient
    {
        Task<PricingResponse> GetPricingAsync(PricingRequest request, CancellationToken token);
    }
}