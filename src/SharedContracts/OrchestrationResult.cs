using FormularyApi;

namespace SharedContracts;

public class OrchestrationResult
{   
    public OrchestrationResult(
        ClaimRequest request,
        EligibilityResponse eligibilityResult,
        CoverageResponse coverageResult,
        PricingResponse pricingResult,
        FormularyApi.HelloReply formularyResult,
        long eligibilityTime,
        long coverageTime,
        long pricingTime,
        long formularyTime
        )
    {
        Request = request;
        EligibilityResult = eligibilityResult;
        CoverageResult = coverageResult;
        PricingResult = pricingResult;
        FormularyResult = formularyResult;
        EligibilityTime = eligibilityTime;
        CoverageTime = coverageTime;
        PricingTime = pricingTime;
        FormularyTime = formularyTime;
    }

    public ClaimRequest Request { get; }
    public EligibilityResponse EligibilityResult { get; }
    public CoverageResponse CoverageResult { get; }
    public PricingResponse PricingResult { get; }
    public HelloReply FormularyResult { get; }
    public long EligibilityTime { get; }
    public long CoverageTime { get; }
    public long PricingTime { get; }
    public long FormularyTime { get; }
}

