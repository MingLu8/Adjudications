using SharedContracts;

namespace ApiGateway.Abstractions
{
    public interface IKafkaProducerService 
    { 
        Task SendAsync(ClaimRequest request, CancellationToken token);
    }

}
