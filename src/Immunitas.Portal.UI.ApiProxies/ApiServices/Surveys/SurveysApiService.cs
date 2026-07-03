using System;
using Flurl.Http;
using Immunitas.Portal.UI.ApiProxies.Contracts;

namespace Immunitas.Portal.UI.ApiProxies.ApiServices.Surveys;

public class SurveysApiService(IFlurlClient client) : ISurveysApiService
{
    private readonly string controllerPath = "/api/Surveys";

    private IFlurlRequest FlurlRequest => client.Request(controllerPath);

    public Task<GetSurveysResponse> GetSurveys(GetSurveysRequest request, CancellationToken cancellationToken = default)
    {
        return FlurlRequest
            .SetQueryParams(request)
            .GetJsonAsync<GetSurveysResponse>(cancellationToken: cancellationToken);
    }
}
