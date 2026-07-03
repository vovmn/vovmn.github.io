using Immunitas.Domain.Entities.Users;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Immunitas.Application;

public class UserContext
{
    public int? UserId { get; private set; }
    public string? Email { get; private set; }
    public UserRole Role { get; private set; } = UserRole.Anonymous;

    public void Init(ClaimsPrincipal principal)
    {
        var userIdClaim = principal.FindFirst("sub");
        var emailClaim = principal.FindFirst(ClaimTypes.Email);
        var roleClaim = principal.FindFirst(ClaimTypes.Role);
        if (userIdClaim == null || emailClaim == null || roleClaim == null)
        {
            throw new InvalidOperationException("Invalid claims principal: missing required claims.");
        }
        UserId = int.Parse(userIdClaim.Value);
        Email = emailClaim.Value;
        Role = Enum.Parse<UserRole>(roleClaim.Value);
    }
}
