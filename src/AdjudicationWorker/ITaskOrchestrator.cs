using SharedContracts;

namespace AdjudicationWorker
{
    public interface ITaskOrchestrator
    {
        Task<OrchestrationResult> ProcessClaimRequestAsync(string transactionId, NCPDPClaim claim);
    }
}