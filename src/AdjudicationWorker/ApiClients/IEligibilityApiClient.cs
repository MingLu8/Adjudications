
namespace AdjudicationWorker.ApiClients
{
    public interface IEligibilityApiClient
    {
        Task<EligibilityResponse> GetEligibilityAsync(EligibilityRequest request, CancellationToken token);
    }
}