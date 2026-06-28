using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GasMapQuebec.Pricing.Domain;

namespace GasMapQuebec.Pricing.Infrastructure.Configurations;

internal sealed class PriceHistoryConfiguration : IEntityTypeConfiguration<PriceHistoryEntry>
{
    public void Configure(EntityTypeBuilder<PriceHistoryEntry> builder)
    {
        builder.ToTable("price_history");

        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id).ValueGeneratedNever();

        builder.Property(h => h.StationId);
        builder.Property(h => h.FuelType).HasConversion<string>().HasMaxLength(16);
        builder.Property(h => h.PriceCents).HasPrecision(8, 2);
        builder.Property(h => h.IsAvailable);
        builder.Property(h => h.ObservedAtUtc);

        // Serves the history query: filter by station (+ optional grade) over a time range, time-ordered.
        builder.HasIndex(h => new { h.StationId, h.FuelType, h.ObservedAtUtc });
    }
}
