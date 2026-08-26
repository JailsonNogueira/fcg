using FCG.Application.Common;
using FCG.Application.Games.ActivateGame;
using FCG.Application.Games.DeactivateGame;
using FCG.Application.Games.GetGameById;
using FCG.Application.Games.GetGames;
using FCG.Application.Games.UpdateGame;
using FCG.Domain.Games;
using FCG.Domain.Promotions;
using FCG.Tests.Shared.Fakes;

namespace FCG.Tests.Unit.Application.Games;

public sealed class ManageGamesHandlerTests
{
    private static readonly DateTimeOffset FixedNow = new(2026, 8, 23, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task GetGames_ShouldReturnBasePriceWhenThereIsNoPromotion()
    {
        var game = Game.Create("FIAP Adventure", "Aventura", 100m);
        var handler = new GetGamesHandler(
            new InMemoryGameRepository().Seed(game),
            new InMemoryPromotionRepository(),
            new FixedClock(FixedNow));

        var summary = Assert.Single((await handler.HandleAsync(new GetGamesQuery())).Items);

        Assert.Equal(100m, summary.BasePrice);
        Assert.Equal(100m, summary.CurrentPrice);
        Assert.Null(summary.DiscountPercentage);
    }

    [Fact]
    public async Task GetGames_ShouldApplyTheActivePromotionToTheCurrentPrice()
    {
        var game = Game.Create("FIAP Adventure", "Aventura", 100m);
        var promotion = Promotion.Create(game.Id, 30m, FixedNow.AddDays(-1), FixedNow.AddDays(1));
        var handler = new GetGamesHandler(
            new InMemoryGameRepository().Seed(game),
            new InMemoryPromotionRepository().Seed(promotion),
            new FixedClock(FixedNow));

        var summary = Assert.Single((await handler.HandleAsync(new GetGamesQuery())).Items);

        Assert.Equal(100m, summary.BasePrice);
        Assert.Equal(70m, summary.CurrentPrice);
        Assert.Equal(30m, summary.DiscountPercentage);
    }

    [Fact]
    public async Task GetGames_ShouldHideInactiveGamesByDefault()
    {
        var retired = Game.Create("Retirado", "Fora do catálogo", 10m);
        retired.Deactivate();
        var handler = new GetGamesHandler(
            new InMemoryGameRepository().Seed(Game.Create("Ativo", "No catálogo", 10m), retired),
            new InMemoryPromotionRepository(),
            new FixedClock(FixedNow));

        var result = await handler.HandleAsync(new GetGamesQuery());

        Assert.Equal("Ativo", Assert.Single(result.Items).Name);
    }

    [Fact]
    public async Task GetGames_ShouldFilterByName()
    {
        var handler = new GetGamesHandler(
            new InMemoryGameRepository().Seed(
                Game.Create("FIAP Adventure", "Aventura", 10m),
                Game.Create("Corrida Total", "Corrida", 10m)),
            new InMemoryPromotionRepository(),
            new FixedClock(FixedNow));

        // Busca case-insensitive, resolvida pelo nome normalizado.
        var result = await handler.HandleAsync(new GetGamesQuery(Search: "adventure"));

        Assert.Equal("FIAP Adventure", Assert.Single(result.Items).Name);
    }

    [Fact]
    public async Task GetGameById_ShouldRejectUnknownGame()
    {
        var handler = new GetGameByIdHandler(
            new InMemoryGameRepository(),
            new InMemoryPromotionRepository(),
            new FixedClock(FixedNow));

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.HandleAsync(new GetGameByIdQuery(Guid.NewGuid())));
    }

    [Fact]
    public async Task UpdateGame_ShouldChangeTheCommercialDetails()
    {
        var game = Game.Create("FIAP Adventure", "Aventura", 100m);
        var repository = new InMemoryGameRepository().Seed(game);
        var unitOfWork = new RecordingUnitOfWork();

        await new UpdateGameHandler(repository, unitOfWork)
            .HandleAsync(new UpdateGameCommand(game.Id, "FIAP Adventure 2", "Sequência", 120m));

        Assert.Equal("FIAP Adventure 2", game.Name);
        Assert.Equal("FIAP ADVENTURE 2", game.NormalizedName);
        Assert.Equal(120m, game.BasePrice);
        Assert.True(unitOfWork.WasSaved);
    }

    [Fact]
    public async Task UpdateGame_ShouldAcceptTheGamesOwnName()
    {
        var game = Game.Create("FIAP Adventure", "Aventura", 100m);
        var repository = new InMemoryGameRepository().Seed(game);

        await new UpdateGameHandler(repository, new RecordingUnitOfWork())
            .HandleAsync(new UpdateGameCommand(game.Id, "FIAP Adventure", "Nova descrição", 120m));

        Assert.Equal(120m, game.BasePrice);
    }

    [Fact]
    public async Task UpdateGame_ShouldRejectNameOwnedByAnotherGame()
    {
        var game = Game.Create("FIAP Adventure", "Aventura", 100m);
        var repository = new InMemoryGameRepository()
            .Seed(game, Game.Create("Corrida Total", "Corrida", 50m));

        await Assert.ThrowsAsync<ConflictException>(() =>
            new UpdateGameHandler(repository, new RecordingUnitOfWork())
                .HandleAsync(new UpdateGameCommand(game.Id, "Corrida Total", "Aventura", 100m)));
    }

    [Fact]
    public async Task UpdateGame_ShouldRejectUnknownGame()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            new UpdateGameHandler(new InMemoryGameRepository(), new RecordingUnitOfWork())
                .HandleAsync(new UpdateGameCommand(Guid.NewGuid(), "Nome", "Descrição", 10m)));
    }

    [Fact]
    public async Task DeactivateGame_ShouldRemoveTheGameFromTheCatalog()
    {
        var game = Game.Create("FIAP Adventure", "Aventura", 100m);
        var repository = new InMemoryGameRepository().Seed(game);

        await new DeactivateGameHandler(repository, new RecordingUnitOfWork())
            .HandleAsync(new DeactivateGameCommand(game.Id));

        Assert.False(game.IsActive);
    }

    [Fact]
    public async Task ActivateGame_ShouldPutTheGameBackInTheCatalog()
    {
        var game = Game.Create("FIAP Adventure", "Aventura", 100m);
        game.Deactivate();
        var repository = new InMemoryGameRepository().Seed(game);

        await new ActivateGameHandler(repository, new RecordingUnitOfWork())
            .HandleAsync(new ActivateGameCommand(game.Id));

        Assert.True(game.IsActive);
    }
}
