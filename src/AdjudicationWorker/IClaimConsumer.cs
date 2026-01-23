using SharedContracts;

namespace AdjudicationWorker;

public interface IClaimConsumer
{
    Task<ClaimRequest?> ConsumeAsync(CancellationToken token);
    Task EnsureConsumerGroupExistsAsync();
}
