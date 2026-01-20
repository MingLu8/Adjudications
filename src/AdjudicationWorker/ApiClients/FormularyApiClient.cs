using FormularyApi;

namespace AdjudicationWorker.ApiClients
{
    public class FormularyApiClient(Greeter.GreeterClient client) : IFormularyApiClient
    {
        public async Task<HelloReply> GetFormularyAsync(HelloRequest request, CancellationToken token)
        {
            return await client.SayHelloAsync(request, cancellationToken: token);
        }
    }
}
