namespace SharedContracts;

public class ClaimResponse
{
    public string TransactionId { get; set; } = string.Empty;
    public string NcpdpResponsePayload { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string Status { get; set; } = string.Empty;
    public OrchestrationResult OrchestrationResult { get; set; }
}

public class AdjudicationResult
{

}

public class EligibilityResult
{

}

public class FormularyResult
{
}
