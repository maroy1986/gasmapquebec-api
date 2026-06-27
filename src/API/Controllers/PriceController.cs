using Microsoft.AspNetCore.Mvc;
using Pricing.Application;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class PriceController(IStationQueryService stationQueryService) : ControllerBase
{
    /// <summary>Returns all stations and their latest prices as GeoJSON.</summary>
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var featureCollection = await stationQueryService.GetGeoJsonAsync(cancellationToken);
        return Ok(featureCollection);
    }
}
