using Immunitas.Domain.Entities.Users;
using Immunitas.Domain.Exceptions;
using Immunitas.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Immunitas.Application.Auth.Commands.Logout;

public class LogoutCommandHandler(
    IRepository<User> usersRepository,
    IRepository<RefreshToken> refreshTokensRepository,
    UserContext userContext,
    IChangesSaver changesSaver,
    ILogger<LogoutCommandHandler> logger) : ILogoutCommandHandler
{
    public async Task Handle(CancellationToken cancellationToken)
    {
        var userId = userContext.UserId!.Value;
        var user = await usersRepository.GetById(userId)
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(cancellationToken) ?? throw new EntityNotFoundException(nameof(User), userId);
        user.SecurityStamp = Guid.NewGuid().ToString();
        refreshTokensRepository.RemoveRange(user.RefreshTokens);

        await changesSaver.CommitAsync(cancellationToken);
        logger.LogInformation("Пользователь с id {UserId} вышел из системы", userId);
    }
}
