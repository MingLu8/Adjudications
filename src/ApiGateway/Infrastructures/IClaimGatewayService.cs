using SharedContracts;

namespace ApiGateway.Infrastructures
{
    public interface IClaimGatewayService
    {
        Task<ClaimResponse> ProcessAsync(ClaimRequest claim, CancellationToken userToken);
    }
}