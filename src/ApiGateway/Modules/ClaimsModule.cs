using ApiGateway.Infrastructures;

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
        HttpContext ctx,
        ClaimGatewayService gateway,
        ILoggerFactory loggerFactory,
        CancellationToken token)
    {
        var logger = loggerFactory.CreateLogger("ClaimsModule");
        logger.LogInformation("Received adjudicate request from {RemoteIp}", ctx.Connection.RemoteIpAddress);

        string ncpdp;
        try
        {
            using var reader = new StreamReader(ctx.Request.Body);
            ncpdp = await reader.ReadToEndAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to read request body");
            return Results.StatusCode(400);
        }

        logger.LogDebug("Request payload length {Length} bytes", ncpdp?.Length ?? 0);

        try
        {
            var result = await gateway.ProcessAsync(ncpdp, token);
            logger.LogInformation("Completed processing claim; returning response for request from {RemoteIp}", ctx.Connection.RemoteIpAddress);
            return Results.Text(result);
        }
        catch (TimeoutException)
        {
            logger.LogWarning("Adjudication timed out for request from {RemoteIp}", ctx.Connection.RemoteIpAddress);
            return Results.StatusCode(504);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            logger.LogInformation("Request cancelled by client {RemoteIp}", ctx.Connection.RemoteIpAddress);
            return Results.StatusCode(499); // Client Closed Request (non-standard)
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while processing adjudicate request from {RemoteIp}", ctx.Connection.RemoteIpAddress);
            return Results.StatusCode(500);
        }
    }
}