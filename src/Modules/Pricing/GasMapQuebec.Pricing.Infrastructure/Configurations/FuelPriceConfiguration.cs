using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GasMapQuebec.Pricing.Domain;

namespace GasMapQuebec.Pricing.Infrastructure.Configurations;

internal sealed class FuelPriceConfiguration : IEntityTypeConfiguration<FuelPrice>
{
    public void Configure(EntityTypeBuilder<FuelPrice> builder)
    {
        builder.ToTable("fuel_prices");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.FuelType).HasConversion<string>().HasMaxLength(16);
        builder.Property(p => p.PriceCents).HasPrecision(8, 2);
        builder.Property(p => p.IsAvailable);
        builder.Property(p => p.ObservedAtUtc);

        builder.HasIndex(p => new { p.StationId, p.FuelType }).IsUnique();
    }
}
