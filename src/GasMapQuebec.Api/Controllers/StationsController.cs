using System.IO.Compression;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using GasMapQuebec.Pricing.Application;

namespace GasMapQuebec.Api.Controllers;

[ApiController]
public sealed class StationsController(
    IStationQueryService stationQueryService,
    IPriceRefreshService priceRefreshService) : ControllerBase
{
    private static readonly JsonSerializerOptions GeoJsonOptions = new()
    {
        // Keep accented text (e.g. "Régulier") literal so the payload matches the Régie feed.
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never
    };

    /// <summary>
    /// Gzipped GeoJSON FeatureCollection matching the Régie essence Québec feed shape.
    /// The mobile app switches to our API by pointing its feed URL here — no parser changes.
    /// </summary>
    [HttpGet("/stations.geojson")]
    public async Task GetGeoJson(CancellationToken cancellationToken)
    {
        var featureCollection = await stationQueryService.GetGeoJsonAsync(cancellationToken);

        Response.ContentType = "application/json; charset=utf-8";
        await using var gzip = new GZipStream(Response.Body, CompressionLevel.Optimal);
        await JsonSerializer.SerializeAsync(gzip, featureCollection, GeoJsonOptions, cancellationToken);
    }

    /// <summary>Triggers an immediate price refresh from the feed (for testing/ops).</summary>
    [HttpPost("/stations/refresh")]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        var count = await priceRefreshService.RefreshAsync(cancellationToken);
        return Ok(new { stations = count });
    }
}
