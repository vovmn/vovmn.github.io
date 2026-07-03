using Immunitas.Application.Users.Commands;
using Immunitas.Application.Users.Queries.GetUsers;
using Immunitas.Domain.Entities.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Immunitas.Portal.Bff.Controllers;

[Authorize(Roles = nameof(UserRole.Admin))]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetUsers(
        [FromQuery] GetUsersQuery query,
        [FromServices] IGetUsersQueryHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(
        [FromBody] CreateUserCommand command,
        [FromServices] ICreateUserCommandHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(command, cancellationToken);
        return NoContent();
    }
}
