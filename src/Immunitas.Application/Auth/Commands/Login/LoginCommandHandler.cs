using Immunitas.Application.Exceptions;
using Immunitas.Application.Services.Hashing;
using Immunitas.Application.Services.TokensGeneration;
using Immunitas.Domain.Entities.Users;
using Immunitas.Domain.Exceptions;
using Immunitas.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Immunitas.Application.Auth.Commands.Login;

public class LoginCommandHandler(
    IRepository<User> userRepository,
    IRepository<RefreshToken> refreshTokenRepository,
    IChangesSaver changesSaver,
    IHashingService hashingService,
    IUserTokenService userTokenService,
    ILogger<LoginCommandHandler> logger) : ILoginCommandHandler
{
    public async Task<LoginCommandResult> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var user = await userRepository.FirstOrDefaultAsync(u => u.Email == command.Email, cancellationToken);

        if (user is null || !hashingService.VerifyHash(command.Password, user.PasswordHash))
            throw new UserUnauthorizedException();

        (string accessToken, string refreshToken, DateTime refreshTokenExpiresAt) = userTokenService.GenerateTokens(user);

        var utcNow = DateTime.UtcNow;
        var refreshTokenEntity = new RefreshToken
        {
            CreatedAt = utcNow,
            ExpiresAt = refreshTokenExpiresAt,
            Token = refreshToken,
            UserId = user.Id
        };

        refreshTokenRepository.Add(refreshTokenEntity);

        await changesSaver.CommitAsync(cancellationToken);

        logger.LogInformation("Пользователь с id {UserId} успешно вошел в систему", user.Id);

        return new LoginCommandResult
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }
}
