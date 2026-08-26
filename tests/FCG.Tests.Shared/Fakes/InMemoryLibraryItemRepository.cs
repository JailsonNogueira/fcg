using FCG.Domain.Libraries;

namespace FCG.Tests.Shared.Fakes;

public sealed class InMemoryLibraryItemRepository : ILibraryItemRepository
{
    public List<LibraryItem> Items { get; } = [];

    public InMemoryLibraryItemRepository Seed(params LibraryItem[] items)
    {
        Items.AddRange(items);
        return this;
    }

    public Task<bool> ExistsAsync(Guid playerId, Guid gameId, CancellationToken cancellationToken = default)
        => Task.FromResult(Items.Any(i => i.PlayerId == playerId && i.GameId == gameId));

    public Task<IReadOnlyCollection<LibraryItem>> GetByPlayerIdAsync(
        Guid playerId,
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyCollection<LibraryItem>>(Items.Where(i => i.PlayerId == playerId).ToList());

    public Task AddAsync(LibraryItem libraryItem, CancellationToken cancellationToken = default)
    {
        Items.Add(libraryItem);
        return Task.CompletedTask;
    }
}
