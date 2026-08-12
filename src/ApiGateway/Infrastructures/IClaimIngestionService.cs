using SharedContracts;

namespace ApiGateway.Infrastructures
{
    public interface IClaimIngestionService
    {
        Task<ClaimResponse> ProcessAsync(ClaimRequest claim, CancellationToken userToken);
    }
}