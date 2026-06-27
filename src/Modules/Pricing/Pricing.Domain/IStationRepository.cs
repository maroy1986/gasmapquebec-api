using Shared.Abstractions;

namespace Pricing.Domain;

public interface IStationRepository : IRepository<Station, Guid>
{
    /// <summary>Loads all stations with their prices, tracked, for the refresh upsert.</summary>
    Task<IReadOnlyList<Station>> GetAllWithPricesAsync(CancellationToken cancellationToken = default);

    /// <summary>Loads all stations with their prices, untracked, for read endpoints.</summary>
    Task<IReadOnlyList<Station>> GetAllForReadAsync(CancellationToken cancellationToken = default);
}
