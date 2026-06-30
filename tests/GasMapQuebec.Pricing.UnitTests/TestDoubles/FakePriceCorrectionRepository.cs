using GasMapQuebec.Pricing.Domain;

namespace GasMapQuebec.Pricing.UnitTests.TestDoubles;

/// <summary>In-memory <see cref="IPriceCorrectionRepository"/> backed by a list.</summary>
internal sealed class FakePriceCorrectionRepository : IPriceCorrectionRepository
{
    public List<PriceCorrection> Items { get; }

    public FakePriceCorrectionRepository(params PriceCorrection[] seed) => Items = [.. seed];

    public Task<PriceCorrection?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.FirstOrDefault(c => c.Id == id));

    public Task AddAsync(PriceCorrection entity, CancellationToken cancellationToken = default)
    {
        Items.Add(entity);
        return Task.CompletedTask;
    }

    public void Update(PriceCorrection entity) { }

    public void Remove(PriceCorrection entity) => Items.Remove(entity);

    public Task<int> CountBySubmitterSinceAsync(string submitterId, DateTime sinceUtc, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.Count(c => c.SubmitterId == submitterId && c.SubmittedAtUtc >= sinceUtc));

    public Task<IReadOnlyList<PriceCorrection>> GetLatestAcceptedAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(LatestPerGrade(Items.Where(c => c.Status == PriceCorrectionStatus.Accepted)));

    public Task<IReadOnlyList<PriceCorrection>> GetLatestAcceptedForStationAsync(Guid stationId, CancellationToken cancellationToken = default) =>
        Task.FromResult(LatestPerGrade(
            Items.Where(c => c.Status == PriceCorrectionStatus.Accepted && c.StationId == stationId)));

    public Task MarkAcceptedOutdatedAsync(
        IReadOnlyCollection<(Guid StationId, FuelType FuelType)> changed,
        DateTime asOfUtc,
        CancellationToken cancellationToken = default)
    {
        var pairs = changed.ToHashSet();
        foreach (var row in Items.Where(c =>
                     c.Status == PriceCorrectionStatus.Accepted && pairs.Contains((c.StationId, c.FuelType))))
        {
            row.MarkOutdated(asOfUtc);
        }

        return Task.CompletedTask;
    }

    private static IReadOnlyList<PriceCorrection> LatestPerGrade(IEnumerable<PriceCorrection> corrections) =>
        corrections
            .GroupBy(c => (c.StationId, c.FuelType))
            .Select(g => g.OrderByDescending(c => c.SubmittedAtUtc).First())
            .ToList();
}
