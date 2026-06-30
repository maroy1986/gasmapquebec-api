using GasMapQuebec.Pricing.Application;
using GasMapQuebec.Pricing.Application.Contracts;
using GasMapQuebec.Pricing.Domain;
using GasMapQuebec.Pricing.UnitTests.TestDoubles;
using Microsoft.Extensions.Options;

namespace GasMapQuebec.Pricing.UnitTests;

public class PriceCorrectionServiceTests
{
    private const string SubmitterId = "device-1";

    [Fact]
    public async Task Submit_unknown_fuel_type_fails_validation()
    {
        var station = StationFactory.WithPrices((FuelType.Regular, 160.0m, true));
        var (service, _) = CreateService(station);

        var result = await service.SubmitAsync(
            new SubmitPriceCorrectionRequest(station.Id, "premium", 165.0m), SubmitterId);

        Assert.True(result.IsFailure);
        Assert.Equal("Validation", result.Error.Code);
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(2000)]
    public async Task Submit_price_out_of_bounds_fails_validation(double price)
    {
        var station = StationFactory.WithPrices((FuelType.Regular, 160.0m, true));
        var (service, _) = CreateService(station);

        var result = await service.SubmitAsync(
            new SubmitPriceCorrectionRequest(station.Id, "regular", (decimal)price), SubmitterId);

        Assert.True(result.IsFailure);
        Assert.Equal("Validation", result.Error.Code);
    }

    [Fact]
    public async Task Submit_unknown_station_returns_not_found()
    {
        var (service, _) = CreateService();

        var result = await service.SubmitAsync(
            new SubmitPriceCorrectionRequest(Guid.NewGuid(), "regular", 165.0m), SubmitterId);

        Assert.True(result.IsFailure);
        Assert.Equal("NotFound", result.Error.Code);
    }

    [Fact]
    public async Task Submit_small_change_is_accepted_and_persisted()
    {
        var station = StationFactory.WithPrices((FuelType.Regular, 160.0m, true));
        var (service, repo) = CreateService(station);

        var result = await service.SubmitAsync(
            new SubmitPriceCorrectionRequest(station.Id, "regular", 165.0m), SubmitterId);

        Assert.True(result.IsSuccess);
        Assert.Equal(nameof(PriceCorrectionStatus.Accepted), result.Value.Status);

        var stored = Assert.Single(repo.Items);
        Assert.Equal(PriceCorrectionStatus.Accepted, stored.Status);
        Assert.Equal(SubmitterId, stored.SubmitterId);
        Assert.Equal(160.0m, stored.PreviousPriceCents);
    }

    [Fact]
    public async Task Submit_large_change_is_queued_as_pending()
    {
        var station = StationFactory.WithPrices((FuelType.Regular, 160.0m, true));
        var (service, repo) = CreateService(station);

        var result = await service.SubmitAsync(
            new SubmitPriceCorrectionRequest(station.Id, "regular", 200.0m), SubmitterId);

        Assert.True(result.IsSuccess);
        Assert.Equal(nameof(PriceCorrectionStatus.Pending), result.Value.Status);
        Assert.Equal(PriceCorrectionStatus.Pending, Assert.Single(repo.Items).Status);
    }

    [Fact]
    public async Task Submit_for_grade_without_official_price_is_accepted()
    {
        // Station has a Regular price but no Diesel; a Diesel correction has no baseline.
        var station = StationFactory.WithPrices((FuelType.Regular, 160.0m, true));
        var (service, repo) = CreateService(station);

        var result = await service.SubmitAsync(
            new SubmitPriceCorrectionRequest(station.Id, "diesel", 210.0m), SubmitterId);

        Assert.True(result.IsSuccess);
        Assert.Equal(nameof(PriceCorrectionStatus.Accepted), result.Value.Status);
        Assert.Null(Assert.Single(repo.Items).PreviousPriceCents);
    }

    private static (PriceCorrectionService Service, FakePriceCorrectionRepository Repo) CreateService(
        params Station[] stations)
    {
        var repo = new FakePriceCorrectionRepository();
        var service = new PriceCorrectionService(
            new FakeStationRepository(stations),
            repo,
            new FakePricingUnitOfWork(),
            Options.Create(new PriceCorrectionOptions()));

        return (service, repo);
    }
}
