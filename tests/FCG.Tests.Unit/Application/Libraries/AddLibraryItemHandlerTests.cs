using FCG.Application.Abstractions;
using FCG.Application.Common;
using FCG.Application.Libraries.AddLibraryItem;
using FCG.Domain.Games;
using FCG.Domain.Libraries;
using FCG.Domain.Users;
using FCG.Domain.Users.ValueObjects;

namespace FCG.Tests.Unit.Application.Libraries;

public sealed class AddLibraryItemHandlerTests
{
    private static readonly Guid PlayerId = Guid.NewGuid();
    private static readonly Guid GameId = Guid.NewGuid();
    private static readonly DateTimeOffset FixedNow = new(2026, 8, 23, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task HandleAsync_ShouldPersistLibraryItem()
    {
        var users = new StubUserRepository(playerExists: true);
        var games = new StubGameRepository(gameExists: true);
        var library = new InMemoryLibraryItemRepository();
        var handler = new AddLibraryItemHandler(users, games, library, new FixedClock(FixedNow), new RecordingUnitOfWork());

        var id = await handler.HandleAsync(new AddLibraryItemCommand(PlayerId, GameId, 49.90m));

        Assert.NotEqual(Guid.Empty, id);
        Assert.Single(library.AddedItems);
        Assert.Equal(PlayerId, library.AddedItems[0].PlayerId);
        Assert.Equal(GameId, library.AddedItems[0].GameId);
        Assert.Equal(FixedNow, library.AddedItems[0].AcquiredAt);
    }

    [Fact]
    public async Task HandleAsync_ShouldSaveChanges()
    {
        var unitOfWork = new RecordingUnitOfWork();
        var handler = new AddLibraryItemHandler(
            new StubUserRepository(playerExists: true),
            new StubGameRepository(gameExists: true),
            new InMemoryLibraryItemRepository(),
            new FixedClock(FixedNow),
            unitOfWork);

        await handler.HandleAsync(new AddLibraryItemCommand(PlayerId, GameId, 49.90m));

        Assert.True(unitOfWork.WasSaved);
    }

    [Fact]
    public async Task HandleAsync_ShouldRejectUnknownPlayer()
    {
        var handler = new AddLibraryItemHandler(
            new StubUserRepository(playerExists: false),
            new StubGameRepository(gameExists: true),
            new InMemoryLibraryItemRepository(),
            new FixedClock(FixedNow),
            new RecordingUnitOfWork());

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.HandleAsync(new AddLibraryItemCommand(PlayerId, GameId, 49.90m)));
    }

    [Fact]
    public async Task HandleAsync_ShouldRejectUnknownGame()
    {
        var handler = new AddLibraryItemHandler(
            new StubUserRepository(playerExists: true),
            new StubGameRepository(gameExists: false),
            new InMemoryLibraryItemRepository(),
            new FixedClock(FixedNow),
            new RecordingUnitOfWork());

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.HandleAsync(new AddLibraryItemCommand(PlayerId, GameId, 49.90m)));
    }

    [Fact]
    public async Task HandleAsync_ShouldRejectDuplicateLibraryItem()
    {
        var handler = new AddLibraryItemHandler(
            new StubUserRepository(playerExists: true),
            new StubGameRepository(gameExists: true),
            new InMemoryLibraryItemRepository { AlreadyOwned = true },
            new FixedClock(FixedNow),
            new RecordingUnitOfWork());

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.HandleAsync(new AddLibraryItemCommand(PlayerId, GameId, 49.90m)));
    }

    private sealed class StubUserRepository(bool playerExists) : IUserRepository
    {
        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (!playerExists) return Task.FromResult<User?>(null);
            var user = User.CreatePlayer("Player", Email.Create("player@test.com"), "hash");
            return Task.FromResult<User?>(user);
        }

        public Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default) => Task.FromResult<User?>(null);
        public Task<int> CountActiveAdministratorsAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
        public Task AddAsync(User user, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Update(User user) { }
    }

    private sealed class StubGameRepository(bool gameExists) : IGameRepository
    {
        public Task<Game?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var game = gameExists ? Game.Create("Test Game", "Descrição", 49.90m) : null;
            return Task.FromResult(game);
        }

        public Task<bool> ExistsByNormalizedNameAsync(string normalizedName, Guid? ignoredGameId = null, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task AddAsync(Game game, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Update(Game game) { }
    }

    private sealed class InMemoryLibraryItemRepository : ILibraryItemRepository
    {
        public bool AlreadyOwned { get; init; }
        public List<LibraryItem> AddedItems { get; } = [];

        public Task<bool> ExistsAsync(Guid playerId, Guid gameId, CancellationToken cancellationToken = default) =>
            Task.FromResult(AlreadyOwned);

        public Task AddAsync(LibraryItem libraryItem, CancellationToken cancellationToken = default)
        {
            AddedItems.Add(libraryItem);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<LibraryItem>> GetByPlayerIdAsync(Guid playerId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<LibraryItem>>([]);
    }

    private sealed class FixedClock(DateTimeOffset now) : IClock
    {
        public DateTimeOffset UtcNow => now;
    }

    private sealed class RecordingUnitOfWork : IUnitOfWork
    {
        public bool WasSaved { get; private set; }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            WasSaved = true;
            return Task.CompletedTask;
        }
    }
}
