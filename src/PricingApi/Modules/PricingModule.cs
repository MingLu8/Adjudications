using SharedContracts;

namespace PricingApi.Modules;

public static class PricingModule
{
    public static void MapPricingEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("api/v1/pricings", GetPricingAsync);
    }

    private static Task<PricingResponse> GetPricingAsync(
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken token
        )
    {
        Task.Delay(10, token).Wait(token);
        return Task.FromResult(new PricingResponse());
    }
}