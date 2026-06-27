using FuelLog.Application;
using FuelLog.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FuelLog.Infrastructure;

public static class FuelLogModule
{
    public const string DatabaseConnectionName = "gasmapdb";

    public static IHostApplicationBuilder AddFuelLogModule(this IHostApplicationBuilder builder)
    {
        builder.AddNpgsqlDbContext<FuelLogDbContext>(DatabaseConnectionName);

        builder.Services.AddScoped<IFuelLogUnitOfWork>(sp => sp.GetRequiredService<FuelLogDbContext>());
        builder.Services.AddScoped<IFuelLogRepository, FuelLogRepository>();
        builder.Services.AddScoped<IFuelLogService, FuelLogService>();

        return builder;
    }
}
