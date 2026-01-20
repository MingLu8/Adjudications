using AdjudicationWorker.ApiClients;
using FormularyApi;
using SharedContracts;
using System.Diagnostics;

namespace AdjudicationWorker;

public class TaskOrchestrator(
    IEligibilityApiClient eligibilityApiClient,
    ICoverageApiClient coverageApiClient,
    IPricingApiClient pricingApiClient,
    IFormularyApiClient formularyApiClient) : ITaskOrchestrator
{
    public async Task<OrchestrationResult> ProcessClaimRequestAsync(ClaimRequest request)
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        var eligibilityTask = TimeAsync(() => eligibilityApiClient.GetEligibilityAsync(new EligibilityRequest(request.TransactionId), token));

        var coverageTask = TimeAsync(() => coverageApiClient.GetCoverageAsync(new CoverageRequest(request.TransactionId), token));

        var pricingTask = TimeAsync(() => pricingApiClient.GetPricingAsync(new PricingRequest(request.TransactionId), token));

        var formularyTask = TimeAsync(() => formularyApiClient.GetFormularyAsync(new HelloRequest { Name = "AdjudicationWorker" }, token));

        var tasks = new List<Task> { eligibilityTask, coverageTask, pricingTask };

        while (tasks.Count > 0)
        {
            var finished = await Task.WhenAny(tasks);

            if (finished.IsFaulted)
            {
                cts.Cancel();

                await ObserveExeptionsToPreventMemoryLeakAndOtherIssuesAsync(tasks);

                // Rethrow the original failure
                var inner = finished.Exception is AggregateException agg
                    ? agg.InnerException!
                    : finished.Exception!;

                throw inner;
            }

            tasks.Remove(finished);
        }

        var eligibility = await eligibilityTask;
        var coverage = await coverageTask;
        var pricing = await pricingTask;
        var formulary = await formularyTask;

        return new OrchestrationResult(
            request,
            eligibility.Result,
            coverage.Result,
            pricing.Result,
            formulary.Result,
            eligibility.DurationMs,
            coverage.DurationMs,
            pricing.DurationMs,
            formulary.DurationMs);
    }

    // This method ensures that exceptions from all tasks are observed to prevent memory leaks, do not remove.
    private static async Task ObserveExeptionsToPreventMemoryLeakAndOtherIssuesAsync(List<Task> tasks)
    {
        try
        {
            // Observe cancellation exceptions from the remaining tasks
            await Task.WhenAll(tasks);
        }
        catch
        {
            // Expected: other tasks canceled
        }
    }

    private async Task<(T Result, long DurationMs)> TimeAsync<T>(Func<Task<T>> action)
    {
        var sw = Stopwatch.StartNew();
        var result = await action();
        sw.Stop();
        return (result, sw.ElapsedMilliseconds);
    }

}

