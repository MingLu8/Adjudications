namespace PricingApi.Modules;

public static class PricingModule
{
    public static void MapPricingEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("api/v1/pricings", GetPricingAsync);
    }

    private static async Task GetPricingAsync(
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken token
        )
    {
        throw new NotImplementedException();
    }
}