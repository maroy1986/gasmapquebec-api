using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GasMapQuebec.Pricing.Domain;

namespace GasMapQuebec.Pricing.Infrastructure.Configurations;

internal sealed class PriceCorrectionConfiguration : IEntityTypeConfiguration<PriceCorrection>
{
    public void Configure(EntityTypeBuilder<PriceCorrection> builder)
    {
        builder.ToTable("price_corrections");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.StationId);
        builder.Property(c => c.FuelType).HasConversion<string>().HasMaxLength(16);
        builder.Property(c => c.SubmittedPriceCents).HasPrecision(8, 2);
        builder.Property(c => c.PreviousPriceCents).HasPrecision(8, 2);
        builder.Property(c => c.PercentChange).HasPrecision(8, 4);
        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(16);
        builder.Property(c => c.SubmitterId).HasMaxLength(128);
        builder.Property(c => c.SubmittedAtUtc);
        builder.Property(c => c.ReviewedAtUtc);

        // Quota lookups by submitter over a window.
        builder.HasIndex(c => new { c.SubmitterId, c.SubmittedAtUtc });
        // Queue scans and the read-side / supersession lookups.
        builder.HasIndex(c => c.Status);
        builder.HasIndex(c => new { c.StationId, c.FuelType, c.Status });
    }
}
