using FCG.Application.Common;
using FCG.Application.Promotions.CreatePromotion;
using FCG.Domain.Games;
using FCG.Domain.Promotions;
using FCG.Tests.Shared.Fakes;

namespace FCG.Tests.Unit.Application.Promotions;

public sealed class CreatePromotionHandlerTests
{
    private static readonly DateTimeOffset StartsAt = new(2026, 9, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset EndsAt = new(2026, 9, 30, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task HandleAsync_ShouldPersistPromotion()
    {
        var game = Game.Create("Test Game", "Descrição", 99.90m);
        var promotions = new InMemoryPromotionRepository();
        var unitOfWork = new RecordingUnitOfWork();
        var handler = new CreatePromotionHandler(
            new InMemoryGameRepository().Seed(game),
            promotions,
            unitOfWork);

        var id = await handler.HandleAsync(new CreatePromotionCommand(game.Id, 20m, StartsAt, EndsAt));

        Assert.NotEqual(Guid.Empty, id);
        var created = Assert.Single(promotions.Items);
        Assert.Equal(game.Id, created.GameId);
        Assert.Equal(20m, created.DiscountPercentage);
        Assert.True(unitOfWork.WasSaved);
    }

    [Fact]
    public async Task HandleAsync_ShouldRejectUnknownGame()
    {
        var handler = new CreatePromotionHandler(
            new InMemoryGameRepository(),
            new InMemoryPromotionRepository(),
            new RecordingUnitOfWork());

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.HandleAsync(new CreatePromotionCommand(Guid.NewGuid(), 20m, StartsAt, EndsAt)));
    }

    [Fact]
    public async Task HandleAsync_ShouldRejectPromotionOverlappingAnExistingOne()
    {
        var game = Game.Create("Test Game", "Descrição", 99.90m);
        var existing = Promotion.Create(game.Id, 10m, StartsAt, EndsAt);
        var handler = new CreatePromotionHandler(
            new InMemoryGameRepository().Seed(game),
            new InMemoryPromotionRepository().Seed(existing),
            new RecordingUnitOfWork());

        // Começa dentro da vigência da promoção já cadastrada.
        var command = new CreatePromotionCommand(game.Id, 20m, EndsAt.AddDays(-1), EndsAt.AddDays(10));

        await Assert.ThrowsAsync<ConflictException>(() => handler.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_ShouldAcceptPromotionOutsideTheExistingPeriod()
    {
        var game = Game.Create("Test Game", "Descrição", 99.90m);
        var existing = Promotion.Create(game.Id, 10m, StartsAt, EndsAt);
        var promotions = new InMemoryPromotionRepository().Seed(existing);
        var handler = new CreatePromotionHandler(
            new InMemoryGameRepository().Seed(game),
            promotions,
            new RecordingUnitOfWork());

        var command = new CreatePromotionCommand(game.Id, 20m, EndsAt.AddDays(1), EndsAt.AddDays(10));

        await handler.HandleAsync(command);

        Assert.Equal(2, promotions.Items.Count);
    }
}
