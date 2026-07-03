namespace Immunitas.Application.Auth.Commands.Login;

public interface ILoginCommandHandler : IHandler
{
    Task<LoginCommandResult> Handle(LoginCommand command, CancellationToken cancellationToken);
}
