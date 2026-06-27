namespace GasMapQuebec.Shared.Abstractions;

/// <summary>
/// Generic repository contract for an aggregate root <typeparamref name="TEntity"/>
/// identified by <typeparamref name="TId"/>. Module-specific repositories extend this
/// with query methods tailored to their aggregate.
/// </summary>
public interface IRepository<TEntity, in TId>
    where TEntity : AggregateRoot<TId>
    where TId : notnull
{
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    void Update(TEntity entity);

    void Remove(TEntity entity);
}
