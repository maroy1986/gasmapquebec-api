using Pricing.Application.GeoJson;

namespace Pricing.Application;

/// <summary>
/// Read-side queries for stations and prices, including the mobile-compatible
/// GeoJSON projection.
/// </summary>
public interface IStationQueryService
{
    Task<StationFeatureCollection> GetGeoJsonAsync(CancellationToken cancellationToken = default);
}
