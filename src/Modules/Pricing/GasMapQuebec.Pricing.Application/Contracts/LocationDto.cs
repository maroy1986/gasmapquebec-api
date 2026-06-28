namespace GasMapQuebec.Pricing.Application.Contracts;

/// <summary>Named coordinates, to avoid the GeoJSON [longitude, latitude] ordering footgun.</summary>
public sealed record LocationDto(double Latitude, double Longitude);
