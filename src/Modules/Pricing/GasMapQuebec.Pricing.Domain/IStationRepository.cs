using GasMapQuebec.Shared.Abstractions;

namespace GasMapQuebec.Pricing.Domain;

public interface IStationRepository : IRepository<Station, Guid>
{
    /// <summary>Loads all stations with their prices, tracked, for the refresh upsert.</summary>
    Task<IReadOnlyList<Station>> GetAllWithPricesAsync(CancellationToken cancellationToken = default);

    /// <summary>Loads all stations with their prices, untracked, for read endpoints.</summary>
    Task<IReadOnlyList<Station>> GetAllForReadAsync(CancellationToken cancellationToken = default);

    /// <summary>True if a station with the given id exists.</summary>
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
