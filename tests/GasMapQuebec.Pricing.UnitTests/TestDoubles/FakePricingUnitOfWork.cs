using GasMapQuebec.Pricing.Application;

namespace GasMapQuebec.Pricing.UnitTests.TestDoubles;

/// <summary>Records how many times changes were committed.</summary>
internal sealed class FakePricingUnitOfWork : IPricingUnitOfWork
{
    public int SaveChangesCallCount { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCallCount++;
        return Task.FromResult(1);
    }
}
