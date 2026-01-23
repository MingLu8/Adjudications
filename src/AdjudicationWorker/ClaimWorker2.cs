using SharedContracts;

namespace AdjudicationWorker;
public class ClaimWorker2(
    ITaskOrchestrator taskOrchestrator,
    IClaimConsumer consumer,
    INcpdpClaimParser ncpdpClaimParser,
    IClaimResponsePublisher claimResponsePublisher,
    WorkerSettings workerSettings,
    ILogger<ClaimWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Adjudication Worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            ClaimRequest claimRequest;

            try
            {
                claimRequest = await consumer.ConsumeAsync(stoppingToken);   
                var response = await ProcessMessageAsync(claimRequest, stoppingToken);

                if (string.IsNullOrEmpty(claimRequest.NcpdpPayload))
                {
                    logger.LogWarning("Received empty payload from consumer");
                    await claimResponsePublisher.PublishResponseAsync(new ClaimResponse { Status = "Empty ncpdp", TransactionId = claimRequest.TransactionId }, stoppingToken);
                    continue;
                }
                await claimResponsePublisher.PublishResponseAsync(response, stoppingToken);
            }
            catch(NullMessageLengthException)
            {
            }
            catch (StaleMessageException ex)
            {
                logger.LogWarning($"Stale claim detected TransactionId={ex.TransactionId} AgeSeconds={ex.AgeSeconds}");
                await claimResponsePublisher.PublishResponseAsync(new ClaimResponse { Status = "Staled message", TransactionId = ex.TransactionId}, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error consuming message.");
            } 
        }
    }

    private async Task<ClaimResponse> ProcessMessageAsync(
        ClaimRequest claimRequest,
        CancellationToken workerToken)
    {
        NCPDPClaim? claim = null;

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(workerToken);
        cts.CancelAfter(TimeSpan.FromSeconds(workerSettings.ClaimTimeoutSeconds));
        var claimToken = cts.Token;

        try
        {
            claim = ncpdpClaimParser.Parse(claimRequest.NcpdpPayload);

            logger.LogInformation($"Processing Transaction Id={claimRequest.TransactionId}");

            return await ProcessValidClaimAsync(claimRequest.TransactionId, claim, claimToken);
        }
        catch (OperationCanceledException) when (!workerToken.IsCancellationRequested)
        {
            logger.LogWarning($"Claim timed out. transaction Id={claimRequest.TransactionId}");
            return new ClaimResponse
            {
                TransactionId = claimRequest.TransactionId,
                NcpdpResponsePayload = "ERROR|TIMEOUT",
                Success = false
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error processing transaction Id={claimRequest.TransactionId}, error:{ex.Message}.");
            throw;
        }
    }

    // ------------------------------------------------------------
    // VALID CLAIM PROCESSING
    // ------------------------------------------------------------
    private async Task<ClaimResponse> ProcessValidClaimAsync(string transactionId, NCPDPClaim claim, CancellationToken token)
    {
        var orchestrationResult = await taskOrchestrator.ProcessClaimRequestAsync(transactionId, claim);
        await Task.Delay(50, token);

        return new ClaimResponse
        {
            TransactionId = transactionId,
            NcpdpResponsePayload = $"PAID|APPROVED",
            Success = true,
            OrchestrationResult = orchestrationResult
        };
    }
}
