using AacApi.Abstractions;
using AacApi.AppServices;
using AacApi.Extensions;
using AacApi.Infrastructures;
using AacApi.Modules;
using Microsoft.OpenApi;
using RepoDb;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
// --- 1. gRPC & TRANSCODING ---
// AddJsonTranscoding is what turns the .proto 'google.api.http' into REST routes
builder.Services.AddGrpc().AddJsonTranscoding();
builder.Services.AddGrpcReflection();

// This is the bridge that tells Swagger how to read gRPC metadata
builder.Services.AddGrpcSwagger();

// --- 2. SWAGGER / OPENAPI CONFIG ---

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "AacApi", Version = "v1" });
    c.DocInclusionPredicate((docName, apiDesc) => true);
    // This is optional but helps with documentation if you have XML comments
    // var filePath = Path.Combine(AppContext.BaseDirectory, "AacApi.xml");
    // c.IncludeXmlComments(filePath);
});

// --- 3. DEPENDENCY INJECTION ---
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

var app = builder.Build();

// --- 4. MIDDLEWARE PIPELINE ---
app.UseGlobalExceptionHandler();

if (app.Environment.IsDevelopment())
{
    // Generate the JSON using Swashbuckle
    app.UseSwagger(options =>
    {
        options.RouteTemplate = "openapi/{documentName}.json";
    });

    // Scalar UI pointing to the Swashbuckle JSON
    app.MapScalarApiReference(options =>
    {
        // Must match the RouteTemplate above
        options.OpenApiRoutePattern = "/openapi/v1.json";
        options.Title = "AacApi .NET 10";
        options.Theme = ScalarTheme.Moon;

        // Tells Scalar where to send the actual requests
        //options.AddServer("http://localhost:5000");
    });

    app.MapGrpcReflectionService();
}

// --- 5. ENDPOINTS ---
// This maps your gRPC service to the routing engine
app.MapGrpcService<AacGrpcService>();

// Map any other Minimal API endpoints you have
app.MapEndpoints(); 

// --- 6. DATABASE SETUP ---
GlobalConfiguration.Setup().UseSqlServer();

app.Run();