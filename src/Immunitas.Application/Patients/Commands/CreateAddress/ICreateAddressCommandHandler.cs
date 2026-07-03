namespace Immunitas.Application.Patients.Commands.CreateAddress;

public interface ICreateAddressCommandHandler : IHandler
{
    Task<CreateAddressCommandResult> Handle(CreateAddressCommand command, CancellationToken cancellationToken);
}
