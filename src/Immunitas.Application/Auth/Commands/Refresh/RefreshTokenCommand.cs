using Immunitas.Generators.Attributes;

namespace Immunitas.Application.Auth.Commands.Refresh;

[SharedContract]
public class RefreshTokenCommand
{
    public required string OldToken { get; init; }
}
