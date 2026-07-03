using Immunitas.Application.DTOs.DaData;
using Immunitas.Application.Patients.Commands.CreateAddress;
using Immunitas.Application.Patients.Queries.SearchAddresses;
using Immunitas.Application.Services.DaData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Immunitas.Portal.Bff.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AddressesController : ControllerBase
{
    [HttpGet("search")]
    public async Task<IActionResult> SearchAddresses(
        [FromQuery] SearchAddressesQuery query,
        [FromServices] ISearchAddressesQueryHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAddress(
        [FromBody] CreateAddressCommand command,
        [FromServices] ICreateAddressCommandHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(command, cancellationToken);
        return Ok(result);
    }

    [HttpGet("external/search")]
    [AllowAnonymous]
    public async Task<IActionResult> SearchAddressesExternal(
    [FromQuery] string query,
    [FromQuery] int count = 10,
    [FromServices] IDaDataService daDataService = null!,
    CancellationToken cancellationToken = default)
    {
        var result = await daDataService.SearchAddresses(query, count, cancellationToken);

        if (result?.Suggestions == null || !result.Suggestions.Any())
            return Ok(new DaDataDto());

        return Ok(result);
    }
}
