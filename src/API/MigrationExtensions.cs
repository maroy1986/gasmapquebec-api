using FuelLog.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Pricing.Infrastructure;

namespace API;

internal static class MigrationExtensions
{
    /// <summary>
    /// Applies pending EF Core migrations for every module DbContext.
    /// Intended for Development; production should migrate via a controlled step.
    /// </summary>
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        await services.GetRequiredService<PricingDbContext>().Database.MigrateAsync();
        await services.GetRequiredService<FuelLogDbContext>().Database.MigrateAsync();
    }
}
