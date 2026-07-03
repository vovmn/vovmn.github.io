using Immunitas.Portal.UI.ApiProxies.Contracts;

namespace Immunitas.Portal.UI.ApiProxies.ApiServices.CytometerMeasurements
{
    public interface ICytometerMeasurementsApiService : IApiService
    {
        Task<GetCytometerMeasurementsResponse> GetCytometerMeasurements(GetCytometerMeasurementsRequest request, CancellationToken cancellationToken = default);
        Task<CytometerMeasurementDto> GetCytometerMeasurement(int id, CancellationToken cancellationToken = default);
        Task<PerformGmmAnalysisResponse> PerformGmmAnalysis(PerformGmmAnalysisRequest request, CancellationToken cancellationToken = default);
    }
}
