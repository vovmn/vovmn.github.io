using System;
using Immunitas.Application.Surveys.Queries.GetSurveys;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Immunitas.Portal.Bff.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SurveysController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetSurveys(
        [FromQuery] GetSurveysQuery query,
        [FromServices] IGetSurveysQueryHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(query, cancellationToken);
        return Ok(result);
    }
}
