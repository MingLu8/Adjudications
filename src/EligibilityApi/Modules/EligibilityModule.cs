namespace EligibilityApi.Modules;

public static class EligibilityModule
{
    public static void MapEligibilityEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("api/v1/eligibilities", GetEligibilitiesAsync);
    }

    private static async Task GetEligibilitiesAsync(
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken token
        )
    {
        throw new NotImplementedException();
    }
}