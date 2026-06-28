using Microsoft.EntityFrameworkCore;
using GasMapQuebec.Pricing.Application;
using GasMapQuebec.Pricing.Domain;

namespace GasMapQuebec.Pricing.Infrastructure;

public sealed class PricingDbContext(DbContextOptions<PricingDbContext> options)
    : DbContext(options), IPricingUnitOfWork
{
    public const string Schema = "pricing";

    public DbSet<Station> Stations => Set<Station>();
    public DbSet<PriceHistoryEntry> PriceHistory => Set<PriceHistoryEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PricingDbContext).Assembly);
    }
}
