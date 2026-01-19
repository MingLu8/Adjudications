using AdjudicationWorker.ApiClients;
using Microsoft.AspNetCore.Mvc;

namespace CoverageApi.Modules;

public static class CoverageModule
{
    public static void MapCoverageEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("api/v1/coverages", GetCoverageAsync);
    }

    private static async Task GetCoverageAsync(
        [FromBody] CoverageRequest coverageRequest,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken token
        )
    {
        throw new NotImplementedException();
    }
}