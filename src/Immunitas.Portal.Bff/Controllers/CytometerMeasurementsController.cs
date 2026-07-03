using Immunitas.Application.CytometerMeasurements.Queries.CreateCytometerMeasurement;
using Immunitas.Application.CytometerMeasurements.Queries.GetCytometerMeasurements;
using Immunitas.Application.CytometerMeasurements.Queries.PerformGmmAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Immunitas.Portal.Bff.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CytometerMeasurementsController : Controller
{
    [HttpGet]
    public async Task<IActionResult> GetCytometerMeasurements(
        [FromQuery] GetCytometerMeasurementsQuery query,
        [FromServices] IGetCytometerMeasurementsQueryHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCytometerMeasurement(
        [FromBody] CreateCytometerMeasurementCommand command,
        [FromServices] ICreateCytometerMeasurementCommandHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(command, cancellationToken);
        return NoContent();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCytometerMeasurement(
        int id,
        [FromServices] IGetCytometerMeasurementQuery handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(id, cancellationToken);
        return Ok(result);
    }

    [HttpGet(nameof(PerformGmmAnalysis))]
    public async Task<IActionResult> PerformGmmAnalysis(
        [FromQuery] PerformGmmAnalysisQuery query,
        [FromServices] IPerformGmmAnalysisQueryHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(query, cancellationToken);
        return Ok(result);
    }
}
