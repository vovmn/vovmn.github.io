using Immunitas.Application.Exceptions;
using Immunitas.Application.Services.TokensGeneration;
using Immunitas.Domain.Entities.Users;
using Immunitas.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Immunitas.Application.Auth.Commands.Refresh;

public class RefreshTokenCommandHandler(
    IRepository<User> usersRepository,
    IRepository<RefreshToken> refreshTokensRepository,
    IUserTokenService userTokenService,
    UserContext userContext,
    IChangesSaver changesSaver,
    ILogger<RefreshTokenCommandHandler> logger) : IRefreshTokenCommandHandler
{
    public async Task<RefreshTokenCommandResult> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var userTokens = refreshTokensRepository.Where(t => t.UserId == userContext.UserId);

        var refreshToken = userTokens.FirstOrDefault(t => t.Token == command.OldToken)
            ?? throw new UserUnauthorizedException();

        var utcNow = DateTime.UtcNow;

        if (refreshToken.IsUsed)
        {
            var allActiveTokens = userTokens
                .Where(t => t.RevokedAt == null);

            foreach (var t in allActiveTokens)
                t.RevokedAt = utcNow;
            await changesSaver.CommitAsync(cancellationToken);

            logger.LogWarning("Refresh-токен был скомрометирован. Все активные токены пользователя {UserId} были отозваны.", userContext.UserId);
            throw new UserUnauthorizedException();
        }

        if (!refreshToken.IsActive(utcNow))
            throw new UserUnauthorizedException();

        refreshToken.IsUsed = true;

        var user = await usersRepository.GetById(userContext.UserId!.Value)
            .FirstOrDefaultAsync(cancellationToken) ?? throw new UserUnauthorizedException();

        var (newAccess, newRefresh, newRefreshExpiresAt) = userTokenService.GenerateTokens(user);

        refreshTokensRepository.Add(new RefreshToken
        {
            CreatedAt = utcNow,
            ExpiresAt = newRefreshExpiresAt,
            Token = newRefresh,
            UserId = user.Id
        });

        await changesSaver.CommitAsync(cancellationToken);
        return new RefreshTokenCommandResult
        {
            AccessToken = newAccess,
            RefreshToken = newRefresh
        };
    }
}
