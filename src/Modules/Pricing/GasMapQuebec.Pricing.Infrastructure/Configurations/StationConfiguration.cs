using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GasMapQuebec.Pricing.Domain;

namespace GasMapQuebec.Pricing.Infrastructure.Configurations;

internal sealed class StationConfiguration : IEntityTypeConfiguration<Station>
{
    public void Configure(EntityTypeBuilder<Station> builder)
    {
        builder.ToTable("stations");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.Property(s => s.CoordinateKey).HasMaxLength(64).IsRequired();
        builder.HasIndex(s => s.CoordinateKey).IsUnique();

        builder.Property(s => s.Name).HasMaxLength(256).IsRequired();
        builder.Property(s => s.Brand).HasMaxLength(128);
        builder.Property(s => s.Status).HasMaxLength(64).IsRequired();
        builder.Property(s => s.Address).HasMaxLength(512).IsRequired();
        builder.Property(s => s.PostalCode).HasMaxLength(16);
        builder.Property(s => s.Region).HasMaxLength(128);

        builder.OwnsOne(s => s.Coordinate, coordinate =>
        {
            coordinate.Property(c => c.Latitude).HasColumnName("latitude");
            coordinate.Property(c => c.Longitude).HasColumnName("longitude");
        });
        builder.Navigation(s => s.Coordinate).IsRequired();

        builder.HasMany(s => s.Prices)
            .WithOne()
            .HasForeignKey(p => p.StationId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(s => s.Prices)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
