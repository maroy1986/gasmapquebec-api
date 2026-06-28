namespace GasMapQuebec.Pricing.Application.Contracts;

/// <summary>
/// Owned API contract for the station list (the v1 alternative to the Régie-shaped
/// GeoJSON feed). Property names are serialized camelCase by the default MVC policy.
/// </summary>
public sealed record StationsResponse(
    DateTime GeneratedAt,
    int Count,
    IReadOnlyList<StationDto> Stations);
