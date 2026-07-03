using Immunitas.Application.Auth.Commands.Login;
using Immunitas.Application.Auth.Commands.Logout;
using Immunitas.Application.Auth.Commands.Refresh;
using Immunitas.Portal.Bff.Consts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Immunitas.Portal.Bff.Controllers;

[ApiController]
[Route("/api/[controller]/[action]")]
public class AuthController : Controller
{
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Login(
        [FromBody] LoginCommand command,
        [FromServices] ILoginCommandHandler loginCommandHandler,
        CancellationToken cancellationToken)
    {
        var result = await loginCommandHandler.Handle(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = AuthPolicies.RefreshTokenPolicy)]
    public async Task<IActionResult> RefreshToken(
        [FromBody] RefreshTokenCommand command,
        [FromServices] IRefreshTokenCommandHandler refreshTokenCommandHandler,
        CancellationToken cancellationToken)
    {
        var result = await refreshTokenCommandHandler.Handle(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Logout(
        [FromServices] ILogoutCommandHandler logoutCommandHandler,
        CancellationToken cancellationToken)
    {
        await logoutCommandHandler.Handle(cancellationToken);
        return Ok();
    }
}