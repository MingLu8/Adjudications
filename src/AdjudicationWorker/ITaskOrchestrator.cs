using SharedContracts;

namespace AdjudicationWorker
{
    public interface ITaskOrchestrator
    {
        Task<OrchestrationResult> ProcessClaimRequestAsync(ClaimRequest request);
    }
}