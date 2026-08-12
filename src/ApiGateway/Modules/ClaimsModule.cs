using ApiGateway.Infrastructures;
using Microsoft.AspNetCore.Mvc;
using SharedContracts;

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
        IClaimIngestionService claimIngestionService,
        ILoggerFactory loggerFactory,
        CancellationToken token)
    {
        var logger = loggerFactory.CreateLogger("ClaimsModule");
        var remoteIp = ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var transactionId = Guid.NewGuid().ToString();
        logger.LogInformation("Adjudicate request received RemoteIp={RemoteIp}, TransactionId={transationId}", remoteIp, transactionId);

        //var ncpdp = await ReadRequestBodyAsync(ctx, logger);
        if (ncpdp is null)
            return Results.StatusCode(400);

        logger.LogDebug("Request payload length Length={Length} bytes", ncpdp.Length);

        try
        {
            var claim = new ClaimRequest(transactionId, ncpdp, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            var result = await claimIngestionService.ProcessAsync(claim, token);
            logger.LogInformation("Adjudication completed RemoteIp={RemoteIp}", remoteIp);
            if (transactionId != result.TransactionId)
            {
                logger.LogError("TransactionId mismatch RemoteIp={RemoteIp}, Expected={Expected}, Actual={Actual}", remoteIp, transactionId, result.TransactionId);
                return Results.InternalServerError(new { transactionId, result });
            }
            return Results.Ok(new { transactionId, result});
        }
        catch(DuplicateClaimSubmissionException ex)
        {
            logger.LogError(ex, $"duplicated claim submission, claim:'{ncpdp}'.");
            return Results.BadRequest(new { Error = "duplicated claim submission.", Claim = ncpdp });
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
}