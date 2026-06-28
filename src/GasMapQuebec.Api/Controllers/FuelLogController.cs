using GasMapQuebec.FuelLog.Application;
using Microsoft.AspNetCore.Mvc;

namespace GasMapQuebec.Api.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class FuelLogController(IFuelLogService fuelLogService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFuelLogEntryRequest request, CancellationToken cancellationToken)
    {
        var created = await fuelLogService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var entry = await fuelLogService.GetByIdAsync(id, cancellationToken);
        return entry is null ? NotFound() : Ok(entry);
    }

    [HttpGet]
    public async Task<IActionResult> GetForUser([FromQuery] Guid userId, CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
        {
            return BadRequest("userId query parameter is required.");
        }

        var entries = await fuelLogService.GetForUserAsync(userId, cancellationToken);
        return Ok(entries);
    }
}
