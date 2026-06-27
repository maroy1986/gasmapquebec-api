namespace GasMapQuebec.Shared.Abstractions;

/// <summary>
/// Commits pending changes within a module's persistence boundary.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
