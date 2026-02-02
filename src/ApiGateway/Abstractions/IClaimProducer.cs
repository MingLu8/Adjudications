using SharedContracts;

namespace ApiGateway.Abstractions
{

    public interface IClaimProducer 
    { 
        Task ProduceAsync(ClaimRequest request, CancellationToken token);
    }

}
