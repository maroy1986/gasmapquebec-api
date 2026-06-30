using GasMapQuebec.Pricing.Domain;

namespace GasMapQuebec.Pricing.UnitTests.TestDoubles;

/// <summary>In-memory <see cref="IStationRepository"/> backed by a list.</summary>
internal sealed class FakeStationRepository : IStationRepository
{
    private readonly List<Station> _stations;

    public FakeStationRepository(params Station[] stations) => _stations = [.. stations];

    public Task<Station?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_stations.FirstOrDefault(s => s.Id == id));

    public Task<IReadOnlyList<Station>> GetAllWithPricesAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Station>>(_stations);

    public Task<IReadOnlyList<Station>> GetAllForReadAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Station>>(_stations);

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_stations.Any(s => s.Id == id));

    public Task AddAsync(Station entity, CancellationToken cancellationToken = default)
    {
        _stations.Add(entity);
        return Task.CompletedTask;
    }

    public void Update(Station entity) { }

    public void Remove(Station entity) => _stations.Remove(entity);
}
