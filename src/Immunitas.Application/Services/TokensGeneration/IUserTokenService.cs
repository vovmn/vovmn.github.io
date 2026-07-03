using Immunitas.Domain.Entities.Users;

namespace Immunitas.Application.Services.TokensGeneration;

public interface IUserTokenService
{
    (string AccessToken, string RefreshToken, DateTime RefreshTokenExpiresAt) GenerateTokens(User user);
}
