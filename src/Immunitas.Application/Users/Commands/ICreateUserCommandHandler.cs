namespace Immunitas.Application.Users.Commands;

public interface ICreateUserCommandHandler : IHandler
{
    Task Handle(CreateUserCommand command, CancellationToken cancellationToken = default);
}
