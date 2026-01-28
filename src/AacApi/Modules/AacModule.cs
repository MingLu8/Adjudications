using AacApi.Abstractions;
using AacApi.Infrastructures;

namespace AacApi.Modules;

public static class AacModule
{    
    public static void MapEndpoints(this IEndpointRouteBuilder app)
    {
        var apiGroup = app.MapGroup("/api/v1").WithTags("Aac Rest Endpoints");

        apiGroup.MapPost("aac", GetAacAsync).WithName("GetAac");
        apiGroup.MapGet("aac/export", ExportAacAsync).WithName("ExportAac");
        apiGroup.MapPost("aac/import", ImportAacAsync).WithName("ImportAac");
    }

    private static async Task ImportAacAsync(
       string state,
       IFileDownloadService downloadService,
       IExcelParserService parser,
       IAacRepository aacRepository,
       ILoggerFactory loggerFactory,
       HttpContext context,
       CancellationToken token
       )
    {
        var sourceUrl = "https://myersandstauffer.com/documents/AL/AL%20AAC%20by%20NDC//AL%20AAC%20by%20NDC.xlsx";
        try
        {
            await using var excelStream = await downloadService.DownloadToMemoryAsync(sourceUrl, token);

            var data = parser.ParsePricingStream(excelStream);
            var aacRecords = data.Select(a=> new Aac(state, a.Ndc, a.Price, a.EffectiveDate)).ToList();
            await aacRepository.SaveAsync(aacRecords);
        }
        catch (Exception ex)
        {
            // Simple error handling for production: return a 400 or 500
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync($"Error processing the drug pricing file, error:{ex.Message}");
        }
    }

    private static async Task<IResult> GetAacAsync(
        AacRequest request,
        IAacRepository  aacRepository,
        ILoggerFactory loggerFactory,
        HttpContext context,
        CancellationToken token
        )
    {
        //var request = new AacRequest { State = state, Ndc = ndc };
            var aac = await aacRepository.GetByStateAndNdcAsync(request.State, request.Ndc);
            return Results.Ok(aac);
    }

    private static async Task ExportAacAsync(
       string state,
       IFileDownloadService downloadService,
       IExcelParserService parser,
       ICsvService csvService,
       ILoggerFactory loggerFactory,
       HttpContext context,
       CancellationToken token
       )
    {
        var sourceUrl = "https://myersandstauffer.com/documents/AL/AL%20AAC%20by%20NDC//AL%20AAC%20by%20NDC.xlsx";
        try
        {
            // 1. Download
            await using var excelStream = await downloadService.DownloadToMemoryAsync(sourceUrl, token);

            // 2. Parse (Excel -> List<DrugPricingData>)
            var data = parser.ParsePricingStream(excelStream);

            // 3. Configure Response for CSV
            context.Response.ContentType = "text/csv";
            context.Response.Headers.Append("Content-Disposition", $"attachment; filename=\"DrugPricing_{DateTime.UtcNow:yyyyMMdd}.csv\"");

            // 4. Stream CSV data directly to the user's browser
            await csvService.WriteRecordsToStreamAsync(data, context.Response.Body);
        }
        catch (Exception ex)
        {
            // Simple error handling for production: return a 400 or 500
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync($"Error processing the drug pricing file, error:{ex.Message}");
        }
    }
}
