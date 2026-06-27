using System.Globalization;
using GasMapQuebec.Pricing.Application.GeoJson;
using GasMapQuebec.Pricing.Domain;

namespace GasMapQuebec.Pricing.Application;

public sealed class StationQueryService(IStationRepository stationRepository) : IStationQueryService
{
    public async Task<StationFeatureCollection> GetGeoJsonAsync(CancellationToken cancellationToken = default)
    {
        var stations = await stationRepository.GetAllForReadAsync(cancellationToken);

        var features = stations
            .Select(station => new StationFeature
            {
                Geometry = new PointGeometry
                {
                    Coordinates = [station.Coordinate.Longitude, station.Coordinate.Latitude]
                },
                Properties = new StationProperties
                {
                    Name = station.Name,
                    Brand = station.Brand,
                    Status = station.Status,
                    Address = station.Address,
                    PostalCode = station.PostalCode,
                    Region = station.Region,
                    Prices = station.Prices
                        .OrderBy(p => p.FuelType)
                        .Select(p => new PriceProperty
                        {
                            GasType = ToGasType(p.FuelType),
                            Price = FormatPrice(p.PriceCents),
                            IsAvailable = p.IsAvailable
                        })
                        .ToList()
                }
            })
            .ToList();

        var generatedAt = stations
            .SelectMany(s => s.Prices)
            .Select(p => p.ObservedAtUtc)
            .DefaultIfEmpty(DateTime.UtcNow)
            .Max();

        return new StationFeatureCollection
        {
            Metadata = new FeedMetadata { GeneratedAt = generatedAt, TotalStations = features.Count },
            Features = features
        };
    }

    private static string ToGasType(FuelType fuelType) => fuelType switch
    {
        FuelType.Super => "Super",
        FuelType.Diesel => "Diesel",
        _ => "Régulier"
    };

    private static string? FormatPrice(decimal? priceCents) =>
        priceCents is null ? null : $"{priceCents.Value.ToString("0.0", CultureInfo.InvariantCulture)}¢";
}
