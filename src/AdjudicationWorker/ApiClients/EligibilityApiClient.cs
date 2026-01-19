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
        {

            return apiCaller.PostAsync<EligibilityRequest, EligibilityResponse>(httpClient, config.RouteTemplate.Replace("{endpoint}", "eligibilities"), request, token);
        }
    }
}
