using Immunitas.Application.DTOs;
using Immunitas.Domain.Entities.Patients;
using Immunitas.Domain.Exceptions;
using Immunitas.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Immunitas.Application.Patients.Queries.GetPatientById
{
    public class GetPatientByIdQueryHandler(
        IRepository<Patient> patientsRepository) : IGetPatientByIdQueryHandler
    {
        public async Task<PatientDto> Handle(int patientId, CancellationToken cancellationToken = default)
        {
            var patient = await patientsRepository
                .GetById(patientId)
                .Select(x => new PatientDto
                {
                    Id = x.Id,
                    FullName = x.FullName,
                    BirthDate = x.BirthDate,
                    Gender = x.Gender
                })
                .SingleOrDefaultAsync(cancellationToken)
                ?? throw new EntityNotFoundException(nameof(Patient), patientId);

            return patient;
        }
    }
}
