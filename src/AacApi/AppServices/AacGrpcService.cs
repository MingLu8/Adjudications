using AacApi.Abstractions;
using AacApi.Extensions;
using Grpc.Core;

namespace AacApi.AppServices
{
    public class AacGrpcService(IAacRepository aacRepository) : AacService.AacServiceBase
    {
        public override async Task<AacResponse> GetAac(AacRequest request, ServerCallContext context)
        {
            var aac = await aacRepository.GetByStateAndNdcAsync(request.State, request.Ndc);
            return new AacResponse
            { 
                State = aac.State,
                Ndc = aac.Ndc,
                Price = aac.Price.ToProtoDecimal(),
                Id = aac.Id.ToString(),
                EffectiveDate = aac.EffectiveDate.ToProtoDate(),
                CreatedDate = aac.CreatedDate.ToProtoDateTime(),
                UpdatedDate = aac.UpdatedDate.ToProtoDateTime(),
                IsActive = aac.IsActive,
            };
        }
    }
}
