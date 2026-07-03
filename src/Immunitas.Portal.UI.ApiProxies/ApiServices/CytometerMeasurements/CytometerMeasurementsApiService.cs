using Flurl.Http;
using Immunitas.Portal.UI.ApiProxies.Contracts;

namespace Immunitas.Portal.UI.ApiProxies.ApiServices.CytometerMeasurements
{
    public class CytometerMeasurementsApiService(IFlurlClient client) : ICytometerMeasurementsApiService
    {
        private readonly string controllerPath = "/api/CytometerMeasurements";

        public Task<CytometerMeasurementDto> GetCytometerMeasurement(int id, CancellationToken cancellationToken = default)
        {
            return client.Request(controllerPath)
                .AppendPathSegment(id)
                .GetJsonAsync<CytometerMeasurementDto>(cancellationToken: cancellationToken);
        }

        public Task<GetCytometerMeasurementsResponse> GetCytometerMeasurements(GetCytometerMeasurementsRequest request, CancellationToken cancellationToken = default)
        {
            return client.Request(controllerPath)
                .SetQueryParams(request)
                .GetJsonAsync<GetCytometerMeasurementsResponse>(cancellationToken: cancellationToken);
        }

        public Task<PerformGmmAnalysisResponse> PerformGmmAnalysis(PerformGmmAnalysisRequest request, CancellationToken cancellationToken = default)
        {
            return client.Request(controllerPath)
                .AppendPathSegment(nameof(PerformGmmAnalysis))
                .SetQueryParams(request)
                .GetJsonAsync<PerformGmmAnalysisResponse>(cancellationToken: cancellationToken);
        }
    }
}
