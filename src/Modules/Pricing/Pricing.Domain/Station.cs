using Shared.Abstractions;

namespace Pricing.Domain;

/// <summary>
/// A gas station and its latest fuel prices. Aggregate root for the Pricing module.
/// </summary>
public sealed class Station : AggregateRoot<Guid>
{
    private readonly List<FuelPrice> _prices = [];

    // EF Core materialization constructor.
    private Station(Guid id) : base(id) { }

    private Station(
        Guid id,
        GeoCoordinate coordinate,
        string name,
        string? brand,
        string status,
        string address,
        string? postalCode,
        string? region) : base(id)
    {
        Coordinate = coordinate;
        CoordinateKey = coordinate.ToKey();
        Name = name;
        Brand = brand;
        Status = status;
        Address = address;
        PostalCode = postalCode;
        Region = region;
    }

    /// <summary>Natural key derived from the coordinate ("lat,lng"); unique per station.</summary>
    public string CoordinateKey { get; private set; } = null!;
    public GeoCoordinate Coordinate { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? Brand { get; private set; }
    public string Status { get; private set; } = null!;
    public string Address { get; private set; } = null!;
    public string? PostalCode { get; private set; }
    public string? Region { get; private set; }

    public IReadOnlyCollection<FuelPrice> Prices => _prices.AsReadOnly();

    public static Station Create(
        GeoCoordinate coordinate,
        string name,
        string? brand,
        string status,
        string address,
        string? postalCode,
        string? region) =>
        new(Guid.CreateVersion7(), coordinate, name, brand, status, address, postalCode, region);

    public void UpdateDetails(string name, string? brand, string status, string address, string? postalCode, string? region)
    {
        Name = name;
        Brand = brand;
        Status = status;
        Address = address;
        PostalCode = postalCode;
        Region = region;
    }

    /// <summary>
    /// Upserts the station's prices from a feed observation: updates the row for each
    /// reported fuel grade, adds new grades, and drops grades no longer reported.
    /// </summary>
    public void ApplyPrices(IReadOnlyCollection<(FuelType FuelType, decimal? PriceCents, bool IsAvailable)> observations, DateTime observedAtUtc)
    {
        foreach (var observation in observations)
        {
            var existing = _prices.FirstOrDefault(p => p.FuelType == observation.FuelType);
            if (existing is null)
            {
                _prices.Add(FuelPrice.Create(Id, observation.FuelType, observation.PriceCents, observation.IsAvailable, observedAtUtc));
            }
            else
            {
                existing.Update(observation.PriceCents, observation.IsAvailable, observedAtUtc);
            }
        }

        var reported = observations.Select(o => o.FuelType).ToHashSet();
        _prices.RemoveAll(p => !reported.Contains(p.FuelType));
    }
}
