using Immunitas.Application.Samples.Queries.GetPatientSamples;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Immunitas.Portal.Bff.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SamplesController : Controller
{
    [HttpGet]
    public async Task<IActionResult> GetPatientSamples(
        [FromQuery] GetPatientSamplesQuery query,
        [FromServices] IGetPatientSamplesQueryHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(query, cancellationToken);
        return Ok(result);
    }
}
