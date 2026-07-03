using Immunitas.Domain.Entities.Users;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Immunitas.Application.Services.TokensGeneration;

public class UserTokenService(IOptions<TokenCreationOptions> tokenCreationOptions) : IUserTokenService
{
    public (string AccessToken, string RefreshToken, DateTime RefreshTokenExpiresAt) GenerateTokens(User user)
    {
        var tokenCreationOptionsValue = tokenCreationOptions.Value;
        var utcNow = DateTime.UtcNow;

        var claims = new[]
        {
            new Claim("stamp", user.SecurityStamp),
            new Claim("sub", user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Уникальный ID токена
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenCreationOptionsValue.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: tokenCreationOptionsValue.Issuer,
            audience: tokenCreationOptionsValue.Audience,
            claims: claims,
            expires: utcNow.AddMinutes(tokenCreationOptionsValue.AccessTokenExpirationMinutes),
            signingCredentials: creds
        );

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        var refreshToken = GenerateSecureRandomString();
        var refreshTokenExpiresAt = utcNow.AddDays(tokenCreationOptionsValue.RefreshTokenExpirationDays);

        return (accessToken, refreshToken, refreshTokenExpiresAt);
    }

    private string GenerateSecureRandomString()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
