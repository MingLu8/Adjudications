using FormularyApi.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi("v1");

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();
var app = builder.Build();

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

app.MapGrpcService<GreeterService>();

app.Run();
