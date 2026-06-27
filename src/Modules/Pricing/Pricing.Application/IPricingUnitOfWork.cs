using Shared.Abstractions;

namespace Pricing.Application;

/// <summary>
/// Module-scoped unit of work. A distinct interface per module keeps each module's
/// persistence boundary independent and avoids DI ambiguity in the shared container.
/// </summary>
public interface IPricingUnitOfWork : IUnitOfWork;
