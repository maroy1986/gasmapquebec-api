using System.Reflection;

namespace GasMapQuebec.ArchitectureTests;

/// <summary>
/// Central references to each module assembly + its root namespace, so the rules
/// below stay readable and there is a single place to register new modules.
/// </summary>
internal static class ModuleReference
{
    public static readonly Assembly SharedAbstractions = typeof(GasMapQuebec.Shared.Abstractions.Entity<>).Assembly;

    public static readonly Assembly PricingDomain = typeof(GasMapQuebec.Pricing.Domain.Station).Assembly;
    public static readonly Assembly PricingApplication = typeof(GasMapQuebec.Pricing.Application.IPriceService).Assembly;
    public static readonly Assembly PricingInfrastructure = typeof(GasMapQuebec.Pricing.Infrastructure.PricingDbContext).Assembly;

    public static readonly Assembly FuelLogDomain = typeof(GasMapQuebec.FuelLog.Domain.FuelLogEntry).Assembly;
    public static readonly Assembly FuelLogApplication = typeof(GasMapQuebec.FuelLog.Application.IFuelLogService).Assembly;
    public static readonly Assembly FuelLogInfrastructure = typeof(GasMapQuebec.FuelLog.Infrastructure.FuelLogDbContext).Assembly;

    public const string PricingNamespace = "GasMapQuebec.Pricing";
    public const string FuelLogNamespace = "GasMapQuebec.FuelLog";

    public const string EntityFrameworkNamespace = "Microsoft.EntityFrameworkCore";
    public const string AspNetCoreNamespace = "Microsoft.AspNetCore";
    public const string HangfireNamespace = "Hangfire";
}
