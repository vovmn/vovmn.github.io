using Immunitas.Application.Patients.Commands.CreatePatient;
using Immunitas.Application.Patients.Queries.GetPatientById;
using Immunitas.Application.Patients.Queries.GetPatients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Immunitas.Portal.Bff.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PatientsController : Controller
{
    [HttpGet]
    public async Task<IActionResult> GetPatients(
        [FromQuery] GetPatientsQuery query,
        [FromServices] IGetPatientsQueryHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{patientId:guid}")]
    public async Task<IActionResult> GetPatient(
        int patientId,
        [FromServices] IGetPatientByIdQueryHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(patientId, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePatient(
        [FromBody] CreatePatientCommand command,
        [FromServices] ICreatePatientCommandHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(command, cancellationToken);
        return Ok(result);
    }
}
