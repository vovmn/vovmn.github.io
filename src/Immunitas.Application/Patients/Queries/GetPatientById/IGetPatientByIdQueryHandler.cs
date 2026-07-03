using Immunitas.Application.DTOs;

namespace Immunitas.Application.Patients.Queries.GetPatientById
{
    public interface IGetPatientByIdQueryHandler : IHandler
    {
        Task<PatientDto> Handle(int patientId, CancellationToken cancellationToken = default);
    }
}
