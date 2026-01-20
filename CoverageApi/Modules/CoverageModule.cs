using AdjudicationWorker.ApiClients;
using Microsoft.AspNetCore.Mvc;
using SharedContracts;

namespace CoverageApi.Modules;

public static class CoverageModule
{
    public static void MapCoverageEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("api/v1/coverages", GetCoverageAsync);
    }

    private static Task<CoverageResponse> GetCoverageAsync(
        [FromBody] CoverageRequest coverageRequest,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken token
        )
    {
        Task.Delay(10, token).Wait(token);
        return Task.FromResult(new CoverageResponse());
    }
}