using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GasMapQuebec.FuelLog.Infrastructure;

/// <summary>
/// Design-time factory so `dotnet ef` can build the context for migrations
/// without booting the Aspire host. The connection string is a design-time
/// placeholder; the real one is injected by Aspire at runtime.
/// </summary>
internal sealed class FuelLogDbContextFactory : IDesignTimeDbContextFactory<FuelLogDbContext>
{
    public FuelLogDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<FuelLogDbContext>()
            .UseNpgsql("Host=localhost;Database=gasmapdb;Username=postgres;Password=postgres")
            .Options;

        return new FuelLogDbContext(options);
    }
}
