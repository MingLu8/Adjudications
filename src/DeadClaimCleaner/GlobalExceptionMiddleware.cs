using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

namespace DeadClaimCleaner;

public class GlobalExceptionMiddleware : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var logger = context.GetLogger<GlobalExceptionMiddleware>();
            logger.LogError(ex, "An unhandled exception occurred during function execution.");

            // Focus on HTTP triggers: return a 500 Internal Server Error
            var request = await context.GetHttpRequestDataAsync();
            if(request != null)
            {
                var response = request.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
                await response.WriteStringAsync("A custom global error occurred. Please try again later.");
                context.GetInvocationResult().Value = response;
            }
        }
    }
}
