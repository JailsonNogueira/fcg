using FCG.Application.Common;
using FCG.Application.Libraries.AddLibraryItem;
using FCG.Domain.Games;
using FCG.Domain.Libraries;
using FCG.Domain.Promotions;
using FCG.Domain.Users;
using FCG.Domain.Users.ValueObjects;
using FCG.Tests.Shared.Fakes;

namespace FCG.Tests.Unit.Application.Libraries;

public sealed class AddLibraryItemHandlerTests
{
    private static readonly DateTimeOffset FixedNow = new(2026, 8, 23, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task HandleAsync_ShouldPersistLibraryItemAtBasePriceWhenThereIsNoPromotion()
    {
        var player = User.CreatePlayer("Player", Email.Create("player@test.com"), "hash");
        var game = Game.Create("Test Game", "Descrição", 49.90m);
        var library = new InMemoryLibraryItemRepository();
        var unitOfWork = new RecordingUnitOfWork();
        var handler = Build(player, game, new InMemoryPromotionRepository(), library, unitOfWork);

        var id = await handler.HandleAsync(new AddLibraryItemCommand(player.Id, game.Id));

        Assert.NotEqual(Guid.Empty, id);
        var item = Assert.Single(library.Items);
        Assert.Equal(player.Id, item.PlayerId);
        Assert.Equal(game.Id, item.GameId);
        Assert.Equal(FixedNow, item.AcquiredAt);
        Assert.Equal(49.90m, item.PricePaid);
        Assert.True(unitOfWork.WasSaved);
    }

    [Fact]
    public async Task HandleAsync_ShouldApplyTheActivePromotionToThePricePaid()
    {
        var player = User.CreatePlayer("Player", Email.Create("player@test.com"), "hash");
        var game = Game.Create("Test Game", "Descrição", 100m);
        var promotion = Promotion.Create(game.Id, 25m, FixedNow.AddDays(-1), FixedNow.AddDays(1));
        var library = new InMemoryLibraryItemRepository();
        var handler = Build(player, game, new InMemoryPromotionRepository().Seed(promotion), library);

        await handler.HandleAsync(new AddLibraryItemCommand(player.Id, game.Id));

        Assert.Equal(75m, Assert.Single(library.Items).PricePaid);
    }

    [Fact]
    public async Task HandleAsync_ShouldIgnorePromotionOutsideItsPeriod()
    {
        var player = User.CreatePlayer("Player", Email.Create("player@test.com"), "hash");
        var game = Game.Create("Test Game", "Descrição", 100m);
        var expired = Promotion.Create(game.Id, 25m, FixedNow.AddDays(-10), FixedNow.AddDays(-5));
        var library = new InMemoryLibraryItemRepository();
        var handler = Build(player, game, new InMemoryPromotionRepository().Seed(expired), library);

        await handler.HandleAsync(new AddLibraryItemCommand(player.Id, game.Id));

        Assert.Equal(100m, Assert.Single(library.Items).PricePaid);
    }

    [Fact]
    public async Task HandleAsync_ShouldRejectUnknownPlayer()
    {
        var game = Game.Create("Test Game", "Descrição", 49.90m);
        var handler = new AddLibraryItemHandler(
            new InMemoryUserRepository(),
            new InMemoryGameRepository().Seed(game),
            new InMemoryPromotionRepository(),
            new InMemoryLibraryItemRepository(),
            new FixedClock(FixedNow),
            new RecordingUnitOfWork());

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.HandleAsync(new AddLibraryItemCommand(Guid.NewGuid(), game.Id)));
    }

    [Fact]
    public async Task HandleAsync_ShouldRejectUnknownGame()
    {
        var player = User.CreatePlayer("Player", Email.Create("player@test.com"), "hash");
        var handler = new AddLibraryItemHandler(
            new InMemoryUserRepository().Seed(player),
            new InMemoryGameRepository(),
            new InMemoryPromotionRepository(),
            new InMemoryLibraryItemRepository(),
            new FixedClock(FixedNow),
            new RecordingUnitOfWork());

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.HandleAsync(new AddLibraryItemCommand(player.Id, Guid.NewGuid())));
    }

    [Fact]
    public async Task HandleAsync_ShouldRejectGameOutOfTheCatalog()
    {
        var player = User.CreatePlayer("Player", Email.Create("player@test.com"), "hash");
        var game = Game.Create("Test Game", "Descrição", 49.90m);
        game.Deactivate();
        var handler = Build(player, game, new InMemoryPromotionRepository(), new InMemoryLibraryItemRepository());

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.HandleAsync(new AddLibraryItemCommand(player.Id, game.Id)));
    }

    [Fact]
    public async Task HandleAsync_ShouldRejectInactiveAccount()
    {
        var player = User.CreatePlayer("Player", Email.Create("player@test.com"), "hash");
        player.Deactivate();
        var game = Game.Create("Test Game", "Descrição", 49.90m);
        var handler = Build(player, game, new InMemoryPromotionRepository(), new InMemoryLibraryItemRepository());

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.HandleAsync(new AddLibraryItemCommand(player.Id, game.Id)));
    }

    [Fact]
    public async Task HandleAsync_ShouldRejectDuplicateLibraryItem()
    {
        var player = User.CreatePlayer("Player", Email.Create("player@test.com"), "hash");
        var game = Game.Create("Test Game", "Descrição", 49.90m);
        var owned = LibraryItem.Create(player.Id, game.Id, FixedNow, 49.90m);
        var handler = Build(
            player,
            game,
            new InMemoryPromotionRepository(),
            new InMemoryLibraryItemRepository().Seed(owned));

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.HandleAsync(new AddLibraryItemCommand(player.Id, game.Id)));
    }

    private static AddLibraryItemHandler Build(
        User player,
        Game game,
        InMemoryPromotionRepository promotions,
        InMemoryLibraryItemRepository library,
        RecordingUnitOfWork? unitOfWork = null)
        => new(
            new InMemoryUserRepository().Seed(player),
            new InMemoryGameRepository().Seed(game),
            promotions,
            library,
            new FixedClock(FixedNow),
            unitOfWork ?? new RecordingUnitOfWork());
}
