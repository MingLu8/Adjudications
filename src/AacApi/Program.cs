using AacApi;
using AacApi.Abstractions;
using AacApi.AppServices;
using AacApi.Extensions;
using AacApi.Infrastructures;
using AacApi.Modules;
using RepoDb;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
// 1. Add gRPC with Transcoding
builder.Services.AddGrpc();//.AddJsonTranscoding();
builder.Services.AddGrpcReflection();

// 2. Add gRPC Swagger Integration
//builder.Services.AddGrpcSwagger();
builder.Services.AddOpenApi("v1");
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IExcelParserService, ExcelParserService>();
builder.Services.AddSingleton<ICsvService, CsvService>();
builder.Services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
builder.Services.AddSingleton<IAacRepository, AacRepository>();
builder.Services.AddHttpClient<IFileDownloadService, FileDownloadService>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(5);
    client.DefaultRequestHeaders.Add("Accept", "application/octet-stream");
})
.AddStandardResilienceHandler();
// Add services to the container.
//builder.Services.AddGrpc();

var app = builder.Build();
app.UseGlobalExceptionHandler();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Modern UI accessible at /scalar
    app.MapScalarApiReference(options =>
    {
        options.OpenApiRoutePattern = "/openapi/v1.json";
        options.Title = "My .NET 10 API";
        options.Theme = ScalarTheme.Moon;
    });
    app.MapGrpcReflectionService();
}
app.MapGrpcService<AacGrpcService>();
app.MapEndpoints();

GlobalConfiguration.Setup().UseSqlServer();
app.Run();
