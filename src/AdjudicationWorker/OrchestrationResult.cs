using AdjudicationWorker.ApiClients;
using SharedContracts;

namespace AdjudicationWorker;

public class OrchestrationResult
{
    public OrchestrationResult(ClaimRequest request, EligibilityResponse eligibilityResult, CoverageResponse coverageResult, PricingResponse pricingResult)
    {
        Request = request;
        EligibilityResult = eligibilityResult;
        CoverageResult = coverageResult;
        PricingResult = pricingResult;
    }

    public ClaimRequest Request { get; }
    public EligibilityResponse EligibilityResult { get; }
    public CoverageResponse CoverageResult { get; }
    public PricingResponse PricingResult { get; }
}

