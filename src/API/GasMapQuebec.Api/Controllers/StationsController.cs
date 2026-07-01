using System.IO.Compression;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using GasMapQuebec.Pricing.Application;
using GasMapQuebec.Pricing.Domain;

namespace GasMapQuebec.Api.Controllers;

[ApiController]
public sealed class StationsController(
    IStationService stationService,
    IPriceHistoryService priceHistoryService,
    IPriceRefreshService priceRefreshService) : ControllerBase
{
    private static readonly JsonSerializerOptions GeoJsonOptions = new()
    {
        // Keep accented text (e.g. "Régulier") literal so the payload matches the Régie feed.
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never
    };

    /// <summary>Owned v1 contract: flat JSON with numeric prices, fuel tokens, and a stable id.</summary>
    [HttpGet("/api/v1/stations")]
    public async Task<IActionResult> GetStations(CancellationToken cancellationToken)
    {
        var response = await stationService.GetStationsAsync(cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Price history for one station, grouped by fuel grade. Defaults to the last 30 days; narrow
    /// or extend with <c>?from=</c>/<c>?to=</c> (ISO-8601 UTC) and filter with <c>?fuelType=</c>.
    /// </summary>
    [HttpGet("/api/v1/stations/{id:guid}/prices/history")]
    public async Task<IActionResult> GetPriceHistory(
        Guid id,
        [FromQuery] string? fuelType,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken)
    {
        FuelType? grade = null;
        if (!string.IsNullOrWhiteSpace(fuelType))
        {
            if (!FuelTypeTokens.TryParse(fuelType, out var parsed))
            {
                return BadRequest($"Unknown fuelType '{fuelType}'. Expected: regular, super, or diesel.");
            }

            grade = parsed;
        }

        var history = await priceHistoryService.GetHistoryAsync(id, grade, from, to, cancellationToken);
        return history is null ? NotFound() : Ok(history);
    }

    /// <summary>
    /// Gzipped GeoJSON FeatureCollection matching the Régie essence Québec feed shape.
    /// The mobile app switches to our API by pointing its feed URL here — no parser changes.
    /// </summary>
    [HttpGet("/stations.geojson")]
    public async Task GetGeoJson(CancellationToken cancellationToken)
    {
        var featureCollection = await stationService.GetGeoJsonAsync(cancellationToken);

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
