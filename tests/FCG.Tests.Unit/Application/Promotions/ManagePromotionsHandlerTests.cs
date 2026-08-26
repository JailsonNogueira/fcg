using FCG.Application.Common;
using FCG.Application.Promotions.ActivatePromotion;
using FCG.Application.Promotions.DeactivatePromotion;
using FCG.Application.Promotions.GetPromotionById;
using FCG.Application.Promotions.GetPromotions;
using FCG.Application.Promotions.UpdatePromotion;
using FCG.Domain.Promotions;
using FCG.Tests.Shared.Fakes;

namespace FCG.Tests.Unit.Application.Promotions;

public sealed class ManagePromotionsHandlerTests
{
    private static readonly Guid GameId = Guid.NewGuid();
    private static readonly DateTimeOffset StartsAt = new(2026, 9, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset EndsAt = new(2026, 9, 30, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task GetPromotions_ShouldHideDisabledPromotionsByDefault()
    {
        var disabled = Promotion.Create(GameId, 10m, StartsAt, EndsAt);
        disabled.Deactivate();
        var repository = new InMemoryPromotionRepository()
            .Seed(Promotion.Create(GameId, 20m, EndsAt.AddDays(1), EndsAt.AddDays(10)), disabled);

        var result = await new GetPromotionsHandler(repository).HandleAsync(new GetPromotionsQuery());

        Assert.Equal(20m, Assert.Single(result.Items).DiscountPercentage);
    }

    [Fact]
    public async Task GetPromotions_ShouldFilterByGame()
    {
        var otherGameId = Guid.NewGuid();
        var repository = new InMemoryPromotionRepository().Seed(
            Promotion.Create(GameId, 20m, StartsAt, EndsAt),
            Promotion.Create(otherGameId, 30m, StartsAt, EndsAt));

        var result = await new GetPromotionsHandler(repository)
            .HandleAsync(new GetPromotionsQuery(GameId: otherGameId));

        Assert.Equal(30m, Assert.Single(result.Items).DiscountPercentage);
    }

    [Fact]
    public async Task GetPromotionById_ShouldRejectUnknownPromotion()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            new GetPromotionByIdHandler(new InMemoryPromotionRepository())
                .HandleAsync(new GetPromotionByIdQuery(Guid.NewGuid())));
    }

    [Fact]
    public async Task UpdatePromotion_ShouldChangeDiscountAndPeriod()
    {
        var promotion = Promotion.Create(GameId, 20m, StartsAt, EndsAt);
        var repository = new InMemoryPromotionRepository().Seed(promotion);
        var unitOfWork = new RecordingUnitOfWork();

        await new UpdatePromotionHandler(repository, unitOfWork)
            .HandleAsync(new UpdatePromotionCommand(promotion.Id, 35m, StartsAt, EndsAt.AddDays(5)));

        Assert.Equal(35m, promotion.DiscountPercentage);
        Assert.Equal(EndsAt.AddDays(5), promotion.EndsAt);
        Assert.True(unitOfWork.WasSaved);
    }

    [Fact]
    public async Task UpdatePromotion_ShouldRejectOverlapWithAnotherPromotionOfTheSameGame()
    {
        var promotion = Promotion.Create(GameId, 20m, StartsAt, EndsAt);
        var neighbour = Promotion.Create(GameId, 30m, EndsAt.AddDays(1), EndsAt.AddDays(10));
        var repository = new InMemoryPromotionRepository().Seed(promotion, neighbour);

        // Estender o término faz a vigência invadir a promoção seguinte.
        await Assert.ThrowsAsync<ConflictException>(() =>
            new UpdatePromotionHandler(repository, new RecordingUnitOfWork())
                .HandleAsync(new UpdatePromotionCommand(promotion.Id, 20m, StartsAt, EndsAt.AddDays(5))));
    }

    [Fact]
    public async Task DeactivatePromotion_ShouldDisableIt()
    {
        var promotion = Promotion.Create(GameId, 20m, StartsAt, EndsAt);
        var repository = new InMemoryPromotionRepository().Seed(promotion);

        await new DeactivatePromotionHandler(repository, new RecordingUnitOfWork())
            .HandleAsync(new DeactivatePromotionCommand(promotion.Id));

        Assert.False(promotion.IsEnabled);
    }

    [Fact]
    public async Task ActivatePromotion_ShouldEnableIt()
    {
        var promotion = Promotion.Create(GameId, 20m, StartsAt, EndsAt);
        promotion.Deactivate();
        var repository = new InMemoryPromotionRepository().Seed(promotion);

        await new ActivatePromotionHandler(repository, new RecordingUnitOfWork())
            .HandleAsync(new ActivatePromotionCommand(promotion.Id));

        Assert.True(promotion.IsEnabled);
    }

    [Fact]
    public async Task ActivatePromotion_ShouldRejectReenablingOverAnOverlappingPromotion()
    {
        var promotion = Promotion.Create(GameId, 20m, StartsAt, EndsAt);
        promotion.Deactivate();
        var replacement = Promotion.Create(GameId, 30m, StartsAt, EndsAt);
        var repository = new InMemoryPromotionRepository().Seed(promotion, replacement);

        await Assert.ThrowsAsync<ConflictException>(() =>
            new ActivatePromotionHandler(repository, new RecordingUnitOfWork())
                .HandleAsync(new ActivatePromotionCommand(promotion.Id)));

        Assert.False(promotion.IsEnabled);
    }
}
