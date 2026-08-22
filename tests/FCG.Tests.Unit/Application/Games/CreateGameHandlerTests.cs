using FCG.Application.Abstractions;
using FCG.Application.Common;
using FCG.Application.Games.CreateGame;
using FCG.Domain.Games;

namespace FCG.Tests.Unit.Application.Games;

public sealed class CreateGameHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldPersistUniqueGame()
    {
        var repository = new InMemoryGameRepository();
        var unitOfWork = new RecordingUnitOfWork();
        var handler = new CreateGameHandler(repository, unitOfWork);

        var gameId = await handler.HandleAsync(new CreateGameCommand("FIAP Adventure", "Aventura", 99.90m));

        Assert.NotEqual(Guid.Empty, gameId);
        Assert.Single(repository.AddedGames);
        Assert.Equal("FIAP ADVENTURE", repository.AddedGames[0].NormalizedName);
        Assert.True(unitOfWork.WasSaved);
    }

    [Fact]
    public async Task HandleAsync_ShouldRejectDuplicateNormalizedName()
    {
        var handler = new CreateGameHandler(new InMemoryGameRepository { NameExists = true }, new RecordingUnitOfWork());

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.HandleAsync(new CreateGameCommand("FIAP Adventure", "Aventura", 99.90m)));
    }

    private sealed class InMemoryGameRepository : IGameRepository
    {
        public bool NameExists { get; init; }
        public List<Game> AddedGames { get; } = [];

        public Task<Game?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult<Game?>(null);

        public Task<bool> ExistsByNormalizedNameAsync(
            string normalizedName,
            Guid? ignoredGameId = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(NameExists);

        public Task AddAsync(Game game, CancellationToken cancellationToken = default)
        {
            AddedGames.Add(game);
            return Task.CompletedTask;
        }

        public void Update(Game game)
        {
        }
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
