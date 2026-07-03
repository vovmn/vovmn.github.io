using Immunitas.Portal.UI.ApiProxies.Contracts;

namespace Immunitas.Portal.UI.ApiProxies.ApiServices.Surveys;

public interface ISurveysApiService : IApiService
{
    Task<GetSurveysResponse> GetSurveys(GetSurveysRequest request, CancellationToken cancellationToken = default);
}
