using Immunitas.Generators.Attributes;

namespace Immunitas.Application.Auth.Commands.Login;

[SharedContract]
public class LoginCommandResult
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
}
