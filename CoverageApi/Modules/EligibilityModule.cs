namespace CoverageApi.Modules;

public static class CoverageModule
{
    public static void MapCoverageEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/coverages", GetCoverageAsync);
    }

    private static async Task GetCoverageAsync(
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken token
        )
    {
        throw new NotImplementedException();
    }
}