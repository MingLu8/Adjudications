using ApiGateway.Infrastructures;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Modules;

public static class ClaimsModule
{
    public static void MapClaimEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/adjudicate", AdjudicateClaim)
           .WithName("AdjudicateClaim")
           .WithSummary("Adjudicate a Pharmacy Claim")
           .WithDescription("Accepts raw NCPDP D.0 string, processes it via Kafka, and returns the response.");
    }

    private static async Task<IResult> AdjudicateClaim(
        [FromBody] string ncpdp,
        HttpContext ctx,
        ClaimGatewayService gateway,
        ILoggerFactory loggerFactory,
        CancellationToken token)
    {
        var logger = loggerFactory.CreateLogger("ClaimsModule");
        var remoteIp = ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var transationId = Guid.NewGuid().ToString();
        logger.LogInformation("Adjudicate request received RemoteIp={RemoteIp}, TransactionId={transationId}", remoteIp, transationId);

        //var ncpdp = await ReadRequestBodyAsync(ctx, logger);
        if (ncpdp is null)
            return Results.StatusCode(400);

        logger.LogDebug("Request payload length Length={Length} bytes", ncpdp.Length);

        try
        {
            var result = await gateway.ProcessAsync(transationId, ncpdp, token);
            logger.LogInformation("Adjudication completed RemoteIp={RemoteIp}", remoteIp);
            if (transationId != result.TransactionId)
            {
                logger.LogError("TransactionId mismatch RemoteIp={RemoteIp}, Expected={Expected}, Actual={Actual}", remoteIp, transationId, result.TransactionId);
                return Results.InternalServerError(new { transationId, result });
            }
            return Results.Ok(new { transationId, result});
        }
        catch (TimeoutException)
        {
            logger.LogWarning("Adjudication timeout RemoteIp={RemoteIp}", remoteIp);
            return Results.StatusCode(504);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            logger.LogInformation("Adjudication canceled by client RemoteIp={RemoteIp}", remoteIp);
            return Results.StatusCode(499);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected adjudication error RemoteIp={RemoteIp}", remoteIp);
            return Results.StatusCode(500);
        }
    }

    private static async Task<string?> ReadRequestBodyAsync(HttpContext ctx, ILogger logger)
    {
        try
        {
            using var reader = new StreamReader(ctx.Request.Body);
            return await reader.ReadToEndAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to read request body");
            return null;
        }
    }

}