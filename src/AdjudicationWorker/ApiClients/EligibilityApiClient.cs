using System;
using System.Collections.Generic;
using System.Text;

namespace AdjudicationWorker.ApiClients
{
    public class EligibilityApiClient(
        HttpClient httpClient,
        EligibilityApiSettings config,
        IApiCaller apiCaller) : IEligibilityApiClient
    {       
        public Task<EligibilityResponse> GetEligibilityAsync(
            EligibilityRequest request,
            CancellationToken token)
            => apiCaller.PostAsync<EligibilityRequest, EligibilityResponse>(httpClient, "get", request, token);
    }
}
