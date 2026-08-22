using FCG.Application.Abstractions;
using FCG.Application.Promotions.CreatePromotion;
using FCG.Domain.Games;
using FCG.Domain.Promotions;

namespace FCG.Tests.Unit.Application.Promotions;

public sealed class CreatePromotionHandlerTests
{
    private static readonly Guid GameId = Guid.NewGuid();
    private static readonly DateTimeOffset StartsAt = new(2026, 9, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset EndsAt = new(2026, 9, 30, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task HandleAsync_ShouldPersistPromotion()
    {
        var games = new StubGameRepository(gameExists: true);
        var promotions = new InMemoryPromotionRepository();
        var handler = new CreatePromotionHandler(games, promotions, new RecordingUnitOfWork());

        var id = await handler.HandleAsync(new CreatePromotionCommand(GameId, 20m, StartsAt, EndsAt));

        Assert.NotEqual(Guid.Empty, id);
        Assert.Single(promotions.AddedPromotions);
        Assert.Equal(GameId, promotions.AddedPromotions[0].GameId);
        Assert.Equal(20m, promotions.AddedPromotions[0].DiscountPercentage);
    }

    [Fact]
    public async Task HandleAsync_ShouldSaveChanges()
    {
        var unitOfWork = new RecordingUnitOfWork();
        var handler = new CreatePromotionHandler(
            new StubGameRepository(gameExists: true),
            new InMemoryPromotionRepository(),
            unitOfWork);

        await handler.HandleAsync(new CreatePromotionCommand(GameId, 20m, StartsAt, EndsAt));

        Assert.True(unitOfWork.WasSaved);
    }

    [Fact]
    public async Task HandleAsync_ShouldRejectUnknownGame()
    {
        var handler = new CreatePromotionHandler(
            new StubGameRepository(gameExists: false),
            new InMemoryPromotionRepository(),
            new RecordingUnitOfWork());

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.HandleAsync(new CreatePromotionCommand(GameId, 20m, StartsAt, EndsAt)));
    }

    private sealed class StubGameRepository(bool gameExists) : IGameRepository
    {
        public Task<Game?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var game = gameExists ? Game.Create("Test Game", "Descrição", 99.90m) : null;
            return Task.FromResult(game);
        }

        public Task<bool> ExistsByNormalizedNameAsync(string normalizedName, Guid? ignoredGameId = null, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task AddAsync(Game game, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Update(Game game) { }
    }

    private sealed class InMemoryPromotionRepository : IPromotionRepository
    {
        public List<Promotion> AddedPromotions { get; } = [];

        public Task AddAsync(Promotion promotion, CancellationToken cancellationToken = default)
        {
            AddedPromotions.Add(promotion);
            return Task.CompletedTask;
        }

        public Task<Promotion?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult<Promotion?>(null);

        public Task<Promotion?> GetActiveByGameIdAsync(Guid gameId, DateTimeOffset referenceTime, CancellationToken cancellationToken = default) =>
            Task.FromResult<Promotion?>(null);

        public void Update(Promotion promotion) { }
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
