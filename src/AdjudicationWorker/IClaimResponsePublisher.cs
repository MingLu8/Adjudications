using SharedContracts;

namespace AdjudicationWorker;

public interface IClaimResponsePublisher
{
    Task PublishResponseAsync(ClaimResponse response, CancellationToken token);
}
