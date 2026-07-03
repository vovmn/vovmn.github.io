namespace Immunitas.Application.Patients.Commands.CreatePatient;

public interface ICreatePatientCommandHandler : IHandler
{
    Task<CreatePatientCommandResult> Handle(CreatePatientCommand command, CancellationToken cancellationToken);
}
