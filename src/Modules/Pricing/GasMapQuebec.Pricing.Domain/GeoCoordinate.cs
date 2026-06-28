using System.Globalization;
using GasMapQuebec.Shared.Abstractions;

namespace GasMapQuebec.Pricing.Domain;

/// <summary>
/// Geographic point (WGS84). Used both for display and to derive the station's
/// natural key, since the upstream feed carries no station identifier.
/// </summary>
public sealed class GeoCoordinate : ValueObject
{
    private GeoCoordinate(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    public double Latitude { get; }
    public double Longitude { get; }

    public static GeoCoordinate Create(double latitude, double longitude)
    {
        if (latitude is < -90 or > 90)
        {
            throw new ArgumentOutOfRangeException(nameof(latitude), latitude, "Latitude must be between -90 and 90.");
        }

        if (longitude is < -180 or > 180)
        {
            throw new ArgumentOutOfRangeException(nameof(longitude), longitude, "Longitude must be between -180 and 180.");
        }

        return new GeoCoordinate(latitude, longitude);
    }

    /// <summary>
    /// Stable "latitude,longitude" key. Mirrors the id the mobile app derives
    /// locally, so the keyless feed can be upserted deterministically.
    /// </summary>
    public string ToKey() =>
        string.Create(CultureInfo.InvariantCulture, $"{Latitude},{Longitude}");

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Latitude;
        yield return Longitude;
    }
}
