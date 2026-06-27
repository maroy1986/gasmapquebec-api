using NetArchTest.Rules;

namespace GasMapQuebec.ArchitectureTests;

/// <summary>
/// Enforces modular-monolith boundaries: modules stay independent of each other,
/// and persistence concerns live only in the infrastructure layer.
/// </summary>
public class ModuleBoundaryTests
{
    [Fact]
    public void Pricing_should_not_depend_on_FuelLog()
    {
        foreach (var assembly in new[]
                 {
                     ModuleReference.PricingDomain,
                     ModuleReference.PricingApplication,
                     ModuleReference.PricingInfrastructure
                 })
        {
            var result = Types.InAssembly(assembly)
                .Should()
                .NotHaveDependencyOn(ModuleReference.FuelLogNamespace)
                .GetResult();

            Assert.True(result.IsSuccessful, $"{assembly.GetName().Name} -> FuelLog:\n" + Describe(result));
        }
    }

    [Fact]
    public void FuelLog_should_not_depend_on_Pricing()
    {
        foreach (var assembly in new[]
                 {
                     ModuleReference.FuelLogDomain,
                     ModuleReference.FuelLogApplication,
                     ModuleReference.FuelLogInfrastructure
                 })
        {
            var result = Types.InAssembly(assembly)
                .Should()
                .NotHaveDependencyOn(ModuleReference.PricingNamespace)
                .GetResult();

            Assert.True(result.IsSuccessful, $"{assembly.GetName().Name} -> Pricing:\n" + Describe(result));
        }
    }

    [Fact]
    public void Repository_implementations_should_live_in_infrastructure()
    {
        foreach (var assembly in new[]
                 {
                     ModuleReference.PricingDomain,
                     ModuleReference.PricingApplication,
                     ModuleReference.FuelLogDomain,
                     ModuleReference.FuelLogApplication
                 })
        {
            var offenders = Types.InAssembly(assembly)
                .That().AreClasses()
                .And().HaveNameEndingWith("Repository")
                .GetTypes()
                .ToList();

            Assert.True(offenders.Count == 0,
                $"Repository implementations must reside in *.Infrastructure, found in {assembly.GetName().Name}:\n"
                + string.Join("\n", offenders.Select(t => t.FullName)));
        }
    }

    private static string Describe(TestResult result) =>
        result.IsSuccessful ? string.Empty : string.Join("\n", result.FailingTypeNames ?? []);
}
