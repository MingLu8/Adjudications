using SharedContracts;

namespace FormularyApi.Modules;

public static class FormularyModule
{
    public static void MapFormulariesEndpoints(this IEndpointRouteBuilder app)
    {
        var apiGroup = app.MapGroup("/api/v1");

        apiGroup.MapPost("api/v1/formularies", GetFormulariesAsync);
    }

    private static Task<HelloReply> GetFormulariesAsync(
        HelloRequest request,
        HttpContext context,
        ILoggerFactory loggerFactory,
        CancellationToken token
        )
    {
        Task.Delay(10, token).Wait(token);
        return Task.FromResult(new HelloReply());
    }
}