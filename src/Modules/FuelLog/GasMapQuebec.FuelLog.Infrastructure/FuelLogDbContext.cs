using GasMapQuebec.FuelLog.Application;
using GasMapQuebec.FuelLog.Domain;
using Microsoft.EntityFrameworkCore;

namespace GasMapQuebec.FuelLog.Infrastructure;

public sealed class FuelLogDbContext(DbContextOptions<FuelLogDbContext> options)
    : DbContext(options), IFuelLogUnitOfWork
{
    public const string Schema = "fuellog";

    public DbSet<FuelLogEntry> Entries => Set<FuelLogEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FuelLogDbContext).Assembly);
    }
}
