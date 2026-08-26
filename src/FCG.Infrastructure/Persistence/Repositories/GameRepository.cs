using FCG.Domain.Games;
using Microsoft.EntityFrameworkCore;

namespace FCG.Infrastructure.Persistence.Repositories;

public sealed class GameRepository(FcgDbContext context) : IGameRepository
{
    public Task<Game?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => context.Games.SingleOrDefaultAsync(g => g.Id == id, cancellationToken);

    public async Task<IReadOnlyCollection<Game>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default)
        => ids.Count == 0
            ? []
            : await context.Games.Where(g => ids.Contains(g.Id)).ToListAsync(cancellationToken);

    public Task<bool> ExistsByNormalizedNameAsync(
        string normalizedName,
        Guid? ignoredGameId = null,
        CancellationToken cancellationToken = default)
        => context.Games.AnyAsync(
            g => g.NormalizedName == normalizedName && (!ignoredGameId.HasValue || g.Id != ignoredGameId),
            cancellationToken);

    public async Task<IReadOnlyCollection<Game>> SearchAsync(
        string? term,
        bool includeInactive,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
        => await Filter(term, includeInactive)
            .OrderBy(g => g.Name)
            .ThenBy(g => g.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(
        string? term,
        bool includeInactive,
        CancellationToken cancellationToken = default)
        => Filter(term, includeInactive).CountAsync(cancellationToken);

    public Task AddAsync(Game game, CancellationToken cancellationToken = default)
        => context.Games.AddAsync(game, cancellationToken).AsTask();

    public void Update(Game game)
        => context.Games.Update(game);

    private IQueryable<Game> Filter(string? term, bool includeInactive)
    {
        var query = context.Games.AsQueryable();

        if (!string.IsNullOrWhiteSpace(term))
        {
            // NormalizedName é o nome em maiúsculas, então a busca fica case-insensitive sem depender do collation.
            var normalizedTerm = term.Trim().ToUpperInvariant();
            query = query.Where(g => g.NormalizedName.Contains(normalizedTerm));
        }

        if (!includeInactive)
        {
            query = query.Where(g => g.IsActive);
        }

        return query;
    }
}
