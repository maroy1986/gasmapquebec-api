using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using GasMapQuebec.Pricing.Application;
using GasMapQuebec.Pricing.Domain;
using GasMapQuebec.Pricing.Infrastructure.Feed;

namespace GasMapQuebec.Pricing.Infrastructure;

public static class PricingModule
{
    public const string DatabaseConnectionName = "gasmapdb";

    public static IHostApplicationBuilder AddPricingModule(this IHostApplicationBuilder builder)
    {
        builder.AddNpgsqlDbContext<PricingDbContext>(DatabaseConnectionName);

        builder.Services.AddScoped<IPricingUnitOfWork>(sp => sp.GetRequiredService<PricingDbContext>());
        builder.Services.AddScoped<IStationRepository, StationRepository>();
        builder.Services.AddScoped<IPriceRepository, PriceRepository>();
        builder.Services.AddScoped<IPriceRefreshService, PriceRefreshService>();
        builder.Services.AddScoped<IStationQueryService, StationQueryService>();

        builder.Services
            .AddOptions<RegieFeedOptions>()
            .Bind(builder.Configuration.GetSection(RegieFeedOptions.SectionName));

        builder.Services.AddHttpClient<IPriceService, RegieEssenceQuebecPriceService>();

        return builder;
    }
}
