namespace Immunitas.Application.Auth.Commands.Refresh;

public interface IRefreshTokenCommandHandler : IHandler
{
    Task<RefreshTokenCommandResult> Handle(RefreshTokenCommand command, CancellationToken cancellationToken);
}
