using Immunitas.Portal.UI.ApiProxies.Contracts;

namespace Immunitas.Portal.UI.ApiProxies.ApiServices.Patients;

/// <summary>
/// Интерфейс для сервиса взаимодействия с пациентами
/// </summary>
public interface IPatientsApiService : IApiService
{
    /// <summary>
    /// Получить список пациентов
    /// </summary>
    /// <param name="request">Запрос на получение пациентов</param>
    /// <returns>Список пациентов и их общее количество</returns>
    Task<GetPatientsResponse> GetPatients(GetPatientsRequest request, CancellationToken cancellationToken = default);

    Task<PatientDto> GetPatientById(int patientId, CancellationToken cancellationToken = default);

    Task<CreatePatientResponse> CreatePatient(CreatePatientRequest command, CancellationToken cancellationToken = default);
}
