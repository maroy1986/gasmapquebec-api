using GasMapQuebec.Pricing.Application.Contracts;
using GasMapQuebec.Pricing.Application.GeoJson;

namespace GasMapQuebec.Pricing.Application;

/// <summary>
/// Read-side queries for stations and prices: the owned v1 contract plus the
/// legacy Régie-compatible GeoJSON projection.
/// </summary>
public interface IStationService
{
    /// <summary>Owned v1 contract: flat JSON with numeric prices and fuel tokens.</summary>
    Task<StationsResponse> GetStationsAsync(CancellationToken cancellationToken = default);

    /// <summary>Legacy Régie essence Québec-shaped GeoJSON feed (mobile-compatible).</summary>
    Task<StationFeatureCollection> GetGeoJsonAsync(CancellationToken cancellationToken = default);
}
