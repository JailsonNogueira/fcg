using FCG.Domain.Games;

namespace FCG.Tests.Shared.Fakes;

public sealed class InMemoryGameRepository : IGameRepository
{
    public List<Game> Items { get; } = [];

    public List<Game> Updated { get; } = [];

    public InMemoryGameRepository Seed(params Game[] games)
    {
        Items.AddRange(games);
        return this;
    }

    public Task<Game?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(Items.SingleOrDefault(g => g.Id == id));

    public Task<IReadOnlyCollection<Game>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyCollection<Game>>(Items.Where(g => ids.Contains(g.Id)).ToList());

    public Task<bool> ExistsByNormalizedNameAsync(
        string normalizedName,
        Guid? ignoredGameId = null,
        CancellationToken cancellationToken = default)
        => Task.FromResult(Items.Any(g => g.NormalizedName == normalizedName && g.Id != ignoredGameId));

    public Task<IReadOnlyCollection<Game>> SearchAsync(
        string? term,
        bool includeInactive,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyCollection<Game>>(
            Filter(term, includeInactive).OrderBy(g => g.Name).Skip(skip).Take(take).ToList());

    public Task<int> CountAsync(
        string? term,
        bool includeInactive,
        CancellationToken cancellationToken = default)
        => Task.FromResult(Filter(term, includeInactive).Count());

    public Task AddAsync(Game game, CancellationToken cancellationToken = default)
    {
        Items.Add(game);
        return Task.CompletedTask;
    }

    public void Update(Game game) => Updated.Add(game);

    private IEnumerable<Game> Filter(string? term, bool includeInactive)
        => Items.Where(g =>
            (string.IsNullOrWhiteSpace(term) || g.NormalizedName.Contains(term.Trim().ToUpperInvariant()))
            && (includeInactive || g.IsActive));
}
