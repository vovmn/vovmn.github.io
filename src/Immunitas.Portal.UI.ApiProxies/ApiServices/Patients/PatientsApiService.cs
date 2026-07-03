using Flurl;
using Flurl.Http;
using Immunitas.Portal.UI.ApiProxies.Contracts;

namespace Immunitas.Portal.UI.ApiProxies.ApiServices.Patients;

/// <summary>
/// API-сервис для взаимодействия с пациентами
/// </summary>
public class PatientsApiService(IFlurlClient client) : IPatientsApiService
{
    private readonly string controllerPath = "/api/Patients";

    private IFlurlRequest FlurlRequest => client.Request(controllerPath);

    /// <inheritdoc/>
    public Task<GetPatientsResponse> GetPatients(GetPatientsRequest request, CancellationToken cancellationToken = default)
    {
        return FlurlRequest
            .SetQueryParams(request)
            .GetJsonAsync<GetPatientsResponse>(cancellationToken: cancellationToken);
    }

    public Task<PatientDto> GetPatientById(int patientId, CancellationToken cancellationToken = default)
    {
        return FlurlRequest
            .AppendPathSegment(patientId)
            .GetJsonAsync<PatientDto>(cancellationToken: cancellationToken);
    }

    public async Task<CreatePatientResponse> CreatePatient(CreatePatientRequest command, CancellationToken cancellationToken = default)
    {
        return await FlurlRequest
            .PostJsonAsync(command, cancellationToken: cancellationToken)
            .ReceiveJson<CreatePatientResponse>();
    }
}