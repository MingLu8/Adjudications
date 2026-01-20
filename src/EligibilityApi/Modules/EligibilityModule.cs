using SharedContracts;

namespace EligibilityApi.Modules;

public static class EligibilityModule
{
    public static void MapEligibilityEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("api/v1/eligibilities", GetEligibilitiesAsync);
    }

    private static Task<EligibilityResponse> GetEligibilitiesAsync(
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken token
        )
    {
        Task.Delay(10, token).Wait(token);
        return Task.FromResult(new EligibilityResponse());
    }
}