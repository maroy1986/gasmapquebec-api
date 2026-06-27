using Microsoft.EntityFrameworkCore;
using Pricing.Application;
using Pricing.Domain;

namespace Pricing.Infrastructure;

public sealed class PricingDbContext(DbContextOptions<PricingDbContext> options)
    : DbContext(options), IPricingUnitOfWork
{
    public const string Schema = "pricing";

    public DbSet<Station> Stations => Set<Station>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PricingDbContext).Assembly);
    }
}
