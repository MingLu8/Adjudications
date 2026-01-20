
using FormularyApi;

namespace AdjudicationWorker.ApiClients
{
    public interface IFormularyApiClient
    {
        Task<HelloReply> GetFormularyAsync(HelloRequest request, CancellationToken token);
    }

}
