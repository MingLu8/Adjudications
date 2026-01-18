using AdjudicationWorker.ApiClients;
using SharedContracts;

namespace AdjudicationWorker;

public class TaskOrchestrator(
    IEligibilityApiClient eligibilityApiClient,
    ICoverageApiClient coverageApiClient,
    IPricingApiClient pricingApiClient) : ITaskOrchestrator
{
    public async Task<OrchestrationResult> ProcessClaimRequestAsync(ClaimRequest request)
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        var eligibilityTask = eligibilityApiClient.GetEligibilityAsync(new EligibilityRequest(), token);
        var coverageTask = coverageApiClient.GetCoverageAsync(new CoverageRequest(), token);
        var pricingTask = pricingApiClient.GetPricingAsync(new PricingRequest(), token);

        var tasks = new List<Task> { eligibilityTask, coverageTask, pricingTask };

        while (tasks.Count > 0)
        {
            var finished = await Task.WhenAny(tasks);

            if (finished.IsFaulted)
            {
                cts.Cancel();
                try { await Task.WhenAll(tasks); } catch { }
                throw new Exception("Fail-fast triggered", finished.Exception);
            }

            tasks.Remove(finished);
        }

        return new OrchestrationResult(request, eligibilityTask.Result, coverageTask.Result, pricingTask.Result);
    }
}

