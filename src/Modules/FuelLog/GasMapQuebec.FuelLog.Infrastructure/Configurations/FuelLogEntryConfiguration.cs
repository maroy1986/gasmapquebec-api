using GasMapQuebec.FuelLog.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GasMapQuebec.FuelLog.Infrastructure.Configurations;

internal sealed class FuelLogEntryConfiguration : IEntityTypeConfiguration<FuelLogEntry>
{
    public void Configure(EntityTypeBuilder<FuelLogEntry> builder)
    {
        builder.ToTable("entries");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.UserId).IsRequired();
        builder.Property(e => e.FilledAtUtc).IsRequired();
        builder.Property(e => e.FuelGrade).HasConversion<string>().HasMaxLength(16);
        builder.Property(e => e.Litres).HasPrecision(8, 2);
        builder.Property(e => e.TotalCost).HasPrecision(10, 2);
        builder.Property(e => e.OdometerKm);
        builder.Property(e => e.StationId);
        builder.Property(e => e.StationName).HasMaxLength(256);
        builder.Property(e => e.Notes).HasMaxLength(1024);

        builder.Ignore(e => e.PricePerLitreCents);

        builder.HasIndex(e => new { e.UserId, e.FilledAtUtc });
    }
}
