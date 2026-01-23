using FormularyApi;

namespace SharedContracts;

public class OrchestrationResult
{   
    public OrchestrationResult(
        NCPDPClaim claim,
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
        Claim = claim;
        EligibilityResult = eligibilityResult;
        CoverageResult = coverageResult;
        PricingResult = pricingResult;
        FormularyResult = formularyResult;
        EligibilityTime = eligibilityTime;
        CoverageTime = coverageTime;
        PricingTime = pricingTime;
        FormularyTime = formularyTime;
    }

    public NCPDPClaim Claim { get; }
    public EligibilityResponse EligibilityResult { get; }
    public CoverageResponse CoverageResult { get; }
    public PricingResponse PricingResult { get; }
    public HelloReply FormularyResult { get; }
    public long EligibilityTime { get; }
    public long CoverageTime { get; }
    public long PricingTime { get; }
    public long FormularyTime { get; }
}

