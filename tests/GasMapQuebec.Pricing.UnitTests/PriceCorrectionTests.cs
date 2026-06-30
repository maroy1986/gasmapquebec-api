using GasMapQuebec.Pricing.Domain;

namespace GasMapQuebec.Pricing.UnitTests;

public class PriceCorrectionTests
{
    private const decimal Threshold = 0.10m;

    [Fact]
    public void Create_below_threshold_is_accepted()
    {
        // 169.9 vs 160.0 official => 6.2% < 10%.
        var correction = PriceCorrection.Create(
            Guid.NewGuid(), FuelType.Regular, submittedPriceCents: 169.9m,
            previousPriceCents: 160.0m, Threshold, "device-1", DateTime.UtcNow);

        Assert.Equal(PriceCorrectionStatus.Accepted, correction.Status);
    }

    [Fact]
    public void Create_at_or_above_threshold_is_pending()
    {
        // 180.0 vs 160.0 official => 12.5% >= 10%.
        var correction = PriceCorrection.Create(
            Guid.NewGuid(), FuelType.Regular, submittedPriceCents: 180.0m,
            previousPriceCents: 160.0m, Threshold, "device-1", DateTime.UtcNow);

        Assert.Equal(PriceCorrectionStatus.Pending, correction.Status);
    }

    [Fact]
    public void Create_exactly_at_threshold_is_pending()
    {
        // 176.0 vs 160.0 => exactly 10%; threshold is inclusive (>=).
        var correction = PriceCorrection.Create(
            Guid.NewGuid(), FuelType.Regular, submittedPriceCents: 176.0m,
            previousPriceCents: 160.0m, Threshold, "device-1", DateTime.UtcNow);

        Assert.Equal(0.10m, correction.PercentChange);
        Assert.Equal(PriceCorrectionStatus.Pending, correction.Status);
    }

    [Fact]
    public void Create_with_no_official_baseline_is_accepted_with_zero_change()
    {
        var correction = PriceCorrection.Create(
            Guid.NewGuid(), FuelType.Diesel, submittedPriceCents: 200.0m,
            previousPriceCents: null, Threshold, "device-1", DateTime.UtcNow);

        Assert.Equal(0m, correction.PercentChange);
        Assert.Equal(PriceCorrectionStatus.Accepted, correction.Status);
    }

    [Fact]
    public void Create_computes_absolute_percent_change_for_a_price_drop()
    {
        // 140.0 vs 160.0 => |−20|/160 = 12.5%.
        var correction = PriceCorrection.Create(
            Guid.NewGuid(), FuelType.Super, submittedPriceCents: 140.0m,
            previousPriceCents: 160.0m, Threshold, "device-1", DateTime.UtcNow);

        Assert.Equal(0.125m, correction.PercentChange);
        Assert.Equal(PriceCorrectionStatus.Pending, correction.Status);
    }

    [Fact]
    public void Approve_sets_accepted_and_records_review_time()
    {
        var correction = PendingCorrection();
        var reviewedAt = DateTime.UtcNow;

        correction.Approve(reviewedAt);

        Assert.Equal(PriceCorrectionStatus.Accepted, correction.Status);
        Assert.Equal(reviewedAt, correction.ReviewedAtUtc);
    }

    [Fact]
    public void Reject_sets_rejected_and_records_review_time()
    {
        var correction = PendingCorrection();
        var reviewedAt = DateTime.UtcNow;

        correction.Reject(reviewedAt);

        Assert.Equal(PriceCorrectionStatus.Rejected, correction.Status);
        Assert.Equal(reviewedAt, correction.ReviewedAtUtc);
    }

    [Fact]
    public void MarkOutdated_sets_outdated_and_records_review_time()
    {
        var correction = PriceCorrection.Create(
            Guid.NewGuid(), FuelType.Regular, 165.0m, 160.0m, Threshold, "device-1", DateTime.UtcNow);
        var asOf = DateTime.UtcNow;

        correction.MarkOutdated(asOf);

        Assert.Equal(PriceCorrectionStatus.Outdated, correction.Status);
        Assert.Equal(asOf, correction.ReviewedAtUtc);
    }

    private static PriceCorrection PendingCorrection() =>
        PriceCorrection.Create(
            Guid.NewGuid(), FuelType.Regular, 200.0m, 160.0m, Threshold, "device-1", DateTime.UtcNow);
}
