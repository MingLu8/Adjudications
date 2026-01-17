namespace ApiGateway.Infrastructures;

using ApiGateway.Abstractions;
using ApiGateway.ConfigurationSettings;
using SharedContracts;

public class ClaimGatewayService(
    IKafkaProducerService producer,
    IResponseMap responseMap,
    KafkaSettings settings,
    ILogger<ClaimGatewayService> logger)
{
    public async Task<string> ProcessAsync(string ncpdpPayload, CancellationToken userToken)
    {
        var claim = new ClaimRequest { NcpdpPayload = ncpdpPayload };
        var tcs = responseMap.Create(claim.TransactionId);

        using var timeoutSource = new CancellationTokenSource(TimeSpan.FromSeconds(settings.TimeoutSeconds));
        using var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(userToken, timeoutSource.Token);

        try
        {
            // Propagate the linked token to the producer
            await producer.SendAsync(claim, linkedSource.Token);

            // Wait for the bridge service to resolve the TCS
            var response = await tcs.Task.WaitAsync(linkedSource.Token);

            return response.NcpdpResponsePayload;
        }
        catch (OperationCanceledException ex)
        {
            bool isTimeout = timeoutSource.IsCancellationRequested;

            // Critical: Ensure the TCS is marked as cancelled so the Bridge doesn't process it late
            tcs.TrySetCanceled(linkedSource.Token);

            if (isTimeout)
            {
                logger.LogWarning("Processing timed out for Transaction {Id} after {Secs}s",
                    claim.TransactionId, settings.TimeoutSeconds);
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
}