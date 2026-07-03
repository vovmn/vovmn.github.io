namespace Immunitas.Application.Auth.Commands.Logout;

public interface ILogoutCommandHandler : IHandler
{
    Task Handle(CancellationToken cancellationToken);
}
