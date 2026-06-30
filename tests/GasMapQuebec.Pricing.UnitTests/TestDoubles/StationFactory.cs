using GasMapQuebec.Pricing.Domain;

namespace GasMapQuebec.Pricing.UnitTests.TestDoubles;

/// <summary>Builds <see cref="Station"/> instances with seeded official prices for tests.</summary>
internal static class StationFactory
{
    public static Station WithPrices(params (FuelType FuelType, decimal? PriceCents, bool IsAvailable)[] prices)
    {
        var station = Station.Create(
            GeoCoordinate.Create(45.5, -73.6),
            name: "Test Station",
            brand: "TestBrand",
            status: "open",
            address: "123 Test St",
            postalCode: "H0H 0H0",
            region: "Montréal");

        if (prices.Length > 0)
        {
            station.ApplyPrices(prices, DateTime.UtcNow);
        }

        return station;
    }
}
