using GasMapQuebec.Pricing.Application;
using GasMapQuebec.Pricing.Domain;
using GasMapQuebec.Pricing.UnitTests.TestDoubles;

namespace GasMapQuebec.Pricing.UnitTests;

public class StationServiceTests
{
    [Fact]
    public async Task GetStations_surfaces_official_and_accepted_community_price()
    {
        var station = StationFactory.WithPrices(
            (FuelType.Regular, 160.0m, true),
            (FuelType.Diesel, 200.0m, true));

        var accepted = PriceCorrection.Create(
            station.Id, FuelType.Regular, submittedPriceCents: 165.0m,
            previousPriceCents: 160.0m, threshold: 0.10m, "device-1", DateTime.UtcNow);

        var service = new StationService(
            new FakeStationRepository(station),
            new FakePriceCorrectionRepository(accepted));

        var response = await service.GetStationsAsync();

        var prices = Assert.Single(response.Stations).Prices;
        var regular = prices.Single(p => p.FuelType == "regular");
        var diesel = prices.Single(p => p.FuelType == "diesel");

        // Official price is always present; the community price rides alongside it.
        Assert.Equal(160.0m, regular.PriceCents);
        Assert.Equal(165.0m, regular.ReportedPriceCents);
        Assert.NotNull(regular.ReportedAt);

        // Diesel has no correction, so no community price is surfaced.
        Assert.Equal(200.0m, diesel.PriceCents);
        Assert.Null(diesel.ReportedPriceCents);
        Assert.Null(diesel.ReportedAt);
    }

    [Fact]
    public async Task GetStations_ignores_non_accepted_corrections()
    {
        var station = StationFactory.WithPrices((FuelType.Regular, 160.0m, true));

        // A large change => Pending, which must not surface as a community price.
        var pending = PriceCorrection.Create(
            station.Id, FuelType.Regular, submittedPriceCents: 200.0m,
            previousPriceCents: 160.0m, threshold: 0.10m, "device-1", DateTime.UtcNow);

        var service = new StationService(
            new FakeStationRepository(station),
            new FakePriceCorrectionRepository(pending));

        var response = await service.GetStationsAsync();

        var regular = Assert.Single(response.Stations).Prices.Single(p => p.FuelType == "regular");
        Assert.Null(regular.ReportedPriceCents);
    }
}
