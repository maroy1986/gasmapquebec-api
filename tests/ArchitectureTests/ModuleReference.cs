using System.Reflection;

namespace ArchitectureTests;

/// <summary>
/// Central references to each module assembly + its root namespace, so the rules
/// below stay readable and there is a single place to register new modules.
/// </summary>
internal static class ModuleReference
{
    public static readonly Assembly SharedAbstractions = typeof(Shared.Abstractions.Entity<>).Assembly;

    public static readonly Assembly PricingDomain = typeof(Pricing.Domain.Station).Assembly;
    public static readonly Assembly PricingApplication = typeof(Pricing.Application.IPriceService).Assembly;
    public static readonly Assembly PricingInfrastructure = typeof(Pricing.Infrastructure.PricingDbContext).Assembly;

    public static readonly Assembly FuelLogDomain = typeof(FuelLog.Domain.FuelLogEntry).Assembly;
    public static readonly Assembly FuelLogApplication = typeof(FuelLog.Application.IFuelLogService).Assembly;
    public static readonly Assembly FuelLogInfrastructure = typeof(FuelLog.Infrastructure.FuelLogDbContext).Assembly;

    public const string PricingNamespace = "Pricing";
    public const string FuelLogNamespace = "FuelLog";

    public const string EntityFrameworkNamespace = "Microsoft.EntityFrameworkCore";
    public const string AspNetCoreNamespace = "Microsoft.AspNetCore";
    public const string HangfireNamespace = "Hangfire";
}
