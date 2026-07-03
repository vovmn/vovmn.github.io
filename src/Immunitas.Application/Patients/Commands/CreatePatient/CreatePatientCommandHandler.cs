using Immunitas.Domain.Entities.Geography;
using Immunitas.Domain.Entities.Patients;
using Immunitas.Domain.Exceptions;
using Immunitas.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Immunitas.Application.Patients.Commands.CreatePatient;

public class CreatePatientCommandHandler(
    IRepository<Patient> patientsRepository,
    IRepository<Address> addressRepository,
    IChangesSaver changesSaver) : ICreatePatientCommandHandler
{
    public async Task<CreatePatientCommandResult> Handle(
        CreatePatientCommand command,
        CancellationToken cancellationToken)
    {

        var address = await addressRepository
            .FirstOrDefaultAsync(a => a.Id == command.AddressId, cancellationToken);

        if (address == null)
            throw new EntityNotFoundException(nameof(Address), command.AddressId);

        var patient = new Patient
        {
            FirstName = command.FirstName,
            LastName = command.LastName,
            MiddleName = command.MiddleName,
            Gender = command.Gender,
            BirthDate = command.BirthDate,
            PhoneNumber = command.PhoneNumber,
            Email = command.Email,
            AddressId = command.AddressId,
            CreatedAt = DateTime.UtcNow
        };

        patientsRepository.Add(patient);
        await changesSaver.CommitAsync(cancellationToken);

        return new CreatePatientCommandResult
        {
            Id = patient.Id
        };
    }
}