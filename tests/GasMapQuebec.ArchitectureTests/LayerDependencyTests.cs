using System.Reflection;
using NetArchTest.Rules;

namespace GasMapQuebec.ArchitectureTests;

/// <summary>
/// Enforces clean-architecture layering inside each module:
/// Domain depends on nothing infrastructural; Application never touches Infrastructure.
/// </summary>
public class LayerDependencyTests
{
    public static TheoryData<Assembly> DomainAssemblies =>
        [ModuleReference.PricingDomain, ModuleReference.FuelLogDomain];

    [Theory]
    [MemberData(nameof(DomainAssemblies))]
    public void Domain_should_not_depend_on_application_infrastructure_or_frameworks(Assembly domainAssembly)
    {
        var result = Types.InAssembly(domainAssembly)
            .Should()
            .NotHaveDependencyOnAny(
                "GasMapQuebec.Pricing.Application",
                "GasMapQuebec.Pricing.Infrastructure",
                "GasMapQuebec.FuelLog.Application",
                "GasMapQuebec.FuelLog.Infrastructure",
                ModuleReference.EntityFrameworkNamespace,
                ModuleReference.AspNetCoreNamespace,
                ModuleReference.HangfireNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Fact]
    public void PricingApplication_should_not_depend_on_infrastructure()
    {
        var result = Types.InAssembly(ModuleReference.PricingApplication)
            .Should()
            .NotHaveDependencyOnAny("GasMapQuebec.Pricing.Infrastructure", ModuleReference.EntityFrameworkNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Fact]
    public void FuelLogApplication_should_not_depend_on_infrastructure()
    {
        var result = Types.InAssembly(ModuleReference.FuelLogApplication)
            .Should()
            .NotHaveDependencyOnAny("GasMapQuebec.FuelLog.Infrastructure", ModuleReference.EntityFrameworkNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    private static string Describe(TestResult result) =>
        result.IsSuccessful
            ? string.Empty
            : "Offending types:\n" + string.Join("\n", result.FailingTypeNames);
}
