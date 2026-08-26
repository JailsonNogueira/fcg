using FCG.Domain.Promotions;

namespace FCG.Tests.Shared.Fakes;

public sealed class InMemoryPromotionRepository : IPromotionRepository
{
    public List<Promotion> Items { get; } = [];

    public List<Promotion> Updated { get; } = [];

    public InMemoryPromotionRepository Seed(params Promotion[] promotions)
    {
        Items.AddRange(promotions);
        return this;
    }

    public Task<Promotion?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(Items.SingleOrDefault(p => p.Id == id));

    public Task<Promotion?> GetActiveByGameIdAsync(
        Guid gameId,
        DateTimeOffset referenceTime,
        CancellationToken cancellationToken = default)
        => Task.FromResult(Items.FirstOrDefault(p => p.GameId == gameId && p.IsActiveAt(referenceTime)));

    public Task<IReadOnlyCollection<Promotion>> GetActiveByGameIdsAsync(
        IReadOnlyCollection<Guid> gameIds,
        DateTimeOffset referenceTime,
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyCollection<Promotion>>(
            Items.Where(p => gameIds.Contains(p.GameId) && p.IsActiveAt(referenceTime)).ToList());

    public Task<bool> ExistsOverlappingAsync(
        Guid gameId,
        DateTimeOffset startsAt,
        DateTimeOffset endsAt,
        Guid? ignoredPromotionId = null,
        CancellationToken cancellationToken = default)
        => Task.FromResult(Items.Any(p =>
            p.GameId == gameId
            && p.IsEnabled
            && p.StartsAt <= endsAt
            && p.EndsAt >= startsAt
            && p.Id != ignoredPromotionId));

    public Task<IReadOnlyCollection<Promotion>> SearchAsync(
        Guid? gameId,
        bool includeDisabled,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyCollection<Promotion>>(
            Filter(gameId, includeDisabled).OrderByDescending(p => p.StartsAt).Skip(skip).Take(take).ToList());

    public Task<int> CountAsync(
        Guid? gameId,
        bool includeDisabled,
        CancellationToken cancellationToken = default)
        => Task.FromResult(Filter(gameId, includeDisabled).Count());

    public Task AddAsync(Promotion promotion, CancellationToken cancellationToken = default)
    {
        Items.Add(promotion);
        return Task.CompletedTask;
    }

    public void Update(Promotion promotion) => Updated.Add(promotion);

    private IEnumerable<Promotion> Filter(Guid? gameId, bool includeDisabled)
        => Items.Where(p => (!gameId.HasValue || p.GameId == gameId.Value) && (includeDisabled || p.IsEnabled));
}
