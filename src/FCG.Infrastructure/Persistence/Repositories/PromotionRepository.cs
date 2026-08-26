using FCG.Domain.Promotions;
using Microsoft.EntityFrameworkCore;

namespace FCG.Infrastructure.Persistence.Repositories;

public sealed class PromotionRepository(FcgDbContext context) : IPromotionRepository
{
    public Task<Promotion?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => context.Promotions.SingleOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<Promotion?> GetActiveByGameIdAsync(
        Guid gameId,
        DateTimeOffset referenceTime,
        CancellationToken cancellationToken = default)
        => context.Promotions.FirstOrDefaultAsync(
            p => p.GameId == gameId && p.IsEnabled && p.StartsAt <= referenceTime && p.EndsAt >= referenceTime,
            cancellationToken);

    public async Task<IReadOnlyCollection<Promotion>> GetActiveByGameIdsAsync(
        IReadOnlyCollection<Guid> gameIds,
        DateTimeOffset referenceTime,
        CancellationToken cancellationToken = default)
        => gameIds.Count == 0
            ? []
            : await context.Promotions
                .Where(p => gameIds.Contains(p.GameId)
                    && p.IsEnabled
                    && p.StartsAt <= referenceTime
                    && p.EndsAt >= referenceTime)
                .ToListAsync(cancellationToken);

    public Task<bool> ExistsOverlappingAsync(
        Guid gameId,
        DateTimeOffset startsAt,
        DateTimeOffset endsAt,
        Guid? ignoredPromotionId = null,
        CancellationToken cancellationToken = default)
        => context.Promotions.AnyAsync(
            p => p.GameId == gameId
                && p.IsEnabled
                && p.StartsAt <= endsAt
                && p.EndsAt >= startsAt
                && (!ignoredPromotionId.HasValue || p.Id != ignoredPromotionId),
            cancellationToken);

    public async Task<IReadOnlyCollection<Promotion>> SearchAsync(
        Guid? gameId,
        bool includeDisabled,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
        => await Filter(gameId, includeDisabled)
            .OrderByDescending(p => p.StartsAt)
            .ThenBy(p => p.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(
        Guid? gameId,
        bool includeDisabled,
        CancellationToken cancellationToken = default)
        => Filter(gameId, includeDisabled).CountAsync(cancellationToken);

    public Task AddAsync(Promotion promotion, CancellationToken cancellationToken = default)
        => context.Promotions.AddAsync(promotion, cancellationToken).AsTask();

    public void Update(Promotion promotion)
        => context.Promotions.Update(promotion);

    private IQueryable<Promotion> Filter(Guid? gameId, bool includeDisabled)
    {
        var query = context.Promotions.AsQueryable();

        if (gameId.HasValue)
        {
            query = query.Where(p => p.GameId == gameId.Value);
        }

        if (!includeDisabled)
        {
            query = query.Where(p => p.IsEnabled);
        }

        return query;
    }
}
