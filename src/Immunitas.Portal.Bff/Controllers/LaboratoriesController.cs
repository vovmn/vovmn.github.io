using Immunitas.Application.Laboratories.Queries.GetLaboratories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Immunitas.Portal.Bff.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LaboratoriesController : ControllerBase
{
    public async Task<IActionResult> GetLaboratories(
        [FromQuery] GetLaboratoriesQuery query,
        [FromServices] IGetLaboratoriesQueryHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(query, cancellationToken);
        return Ok(result);
    }
}
