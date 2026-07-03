using Immunitas.Generators.Attributes;

namespace Immunitas.Application.Auth.Commands.Refresh;

[SharedContract]
public class RefreshTokenCommandResult
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
}
