using Immunitas.Generators.Attributes;

namespace Immunitas.Application.Auth.Commands.Login;

[SharedContract]
public class LoginCommand
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}
