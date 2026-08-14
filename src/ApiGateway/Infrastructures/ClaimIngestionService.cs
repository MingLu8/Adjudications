namespace AdjudicationApi.Infrastructures;

using AdjudicationApi.Abstractions;
using AdjudicationApi.ConfigurationSettings;
using Microsoft.Extensions.Options;
using SharedContracts;

//public class ClaimProcessorSettings
//{
//    public int TimeoutSeconds { get; set; }
//}

//public class ClaimProcessorSettingsValidator : IValidateOptions<ClaimProcessorSettings>
//{
//    public ValidateOptionsResult Validate(string? name, ClaimProcessorSettings options)
//    {
//        return options.TimeoutSeconds > 0
//            ? ValidateOptionsResult.Success
//            : ValidateOptionsResult.Fail("TimeoutSeconds must be greater than zero.");
//    }
//}

public class ClaimIngestionService(
    IDuplicatedSubmissionChecker duplicatedSubmissionChecker,
    IClaimQueue claimQueue,
    IResponseMap responseMap,
    KafkaSettings settings,
    ILogger<ClaimIngestionService> logger) : IClaimIngestionService
{
    public async Task<ClaimResponse> ProcessAsync(
        ClaimRequest claim,
        CancellationToken userToken)
    {
        using var scope = logger.BeginScope(new Dictionary<string, object>
        {
            ["TransactionId"] = claim.TransactionId   
        });

        using var timeoutSource = new CancellationTokenSource(TimeSpan.FromSeconds(settings.TimeoutSeconds));
        using var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(userToken, timeoutSource.Token);

        // Dedup check now bound by the same timeout budget as the rest of the pipeline
        await EnsureNoDuplicatedClaim(claim, linkedSource.Token);

        TaskCompletionSource<ClaimResponse>? tcs = null;

        try
        {
            // Throws DuplicateTransactionException if a collision is found —
            // propagates untouched, not caught below.
            tcs = responseMap.Create(claim.TransactionId);

            try
            {
                await claimQueue.AddAsync(claim, linkedSource.Token);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Failed to produce claim for Transaction {Id}", claim.TransactionId);
                tcs.TrySetCanceled();
                throw new ClaimProducerException($"Failed to submit claim {claim.TransactionId} for processing.", ex);
            }

            var response = await tcs.Task.WaitAsync(linkedSource.Token);
            return response;
        }
        catch (OperationCanceledException ex)
        {
            var isTimeout = timeoutSource.IsCancellationRequested;

            // Critical: Ensure the TCS is marked as cancelled so the Bridge doesn't process it late
            tcs?.TrySetCanceled(linkedSource.Token);

            if (isTimeout)
            {
                logger.LogWarning("Processing timed out for Transaction {Id} after {Secs}s", claim.TransactionId, settings.TimeoutSeconds);
                throw new TimeoutException($"Claim {claim.TransactionId} timed out.", ex);
            }

            logger.LogInformation("Request for Transaction {Id} was cancelled by the user.", claim.TransactionId);
            throw;
        }
        finally
        {
            responseMap.Remove(claim.TransactionId);
        }
    }

    private async Task EnsureNoDuplicatedClaim(ClaimRequest claim, CancellationToken userToken)
    {
        var isUnique = await duplicatedSubmissionChecker.IsUniqueAsync(claim.NcpdpPayload, userToken);
        if (!isUnique)
        {
            logger.LogWarning("Duplicate submission detected for Transaction {Id}", claim.TransactionId);
            throw new DuplicateClaimSubmissionException(claim.NcpdpPayload);
        }
    }
}