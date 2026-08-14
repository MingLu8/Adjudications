using AdjudicationApi.Extensions;
using AdjudicationApi.Modules;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddGatewayInfrastructure(builder.Configuration);

var app = builder.Build();
app.UseGlobalExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapClaimEndpoints();

app.Run();