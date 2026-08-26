using FCG.Application.Libraries.GetPlayerLibrary;
using FCG.Domain.Games;
using FCG.Domain.Libraries;
using FCG.Tests.Shared.Fakes;

namespace FCG.Tests.Unit.Application.Libraries;

public sealed class GetPlayerLibraryHandlerTests
{
    private static readonly Guid PlayerId = Guid.NewGuid();
    private static readonly DateTimeOffset FixedNow = new(2026, 8, 23, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task HandleAsync_ShouldReturnAnEmptyLibrary()
    {
        var handler = new GetPlayerLibraryHandler(
            new InMemoryLibraryItemRepository(),
            new InMemoryGameRepository());

        Assert.Empty(await handler.HandleAsync(new GetPlayerLibraryQuery(PlayerId)));
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnTheGameDataOfEachAcquisition()
    {
        var game = Game.Create("FIAP Adventure", "Aventura", 100m);
        var item = LibraryItem.Create(PlayerId, game.Id, FixedNow, 70m);
        var handler = new GetPlayerLibraryHandler(
            new InMemoryLibraryItemRepository().Seed(item),
            new InMemoryGameRepository().Seed(game));

        var summary = Assert.Single(await handler.HandleAsync(new GetPlayerLibraryQuery(PlayerId)));

        Assert.Equal(game.Id, summary.GameId);
        Assert.Equal("FIAP Adventure", summary.GameName);
        Assert.Equal(70m, summary.PricePaid);
        Assert.Equal(FixedNow, summary.AcquiredAt);
    }

    [Fact]
    public async Task HandleAsync_ShouldNotReturnAnotherPlayersAcquisitions()
    {
        var game = Game.Create("FIAP Adventure", "Aventura", 100m);
        var handler = new GetPlayerLibraryHandler(
            new InMemoryLibraryItemRepository()
                .Seed(LibraryItem.Create(Guid.NewGuid(), game.Id, FixedNow, 100m)),
            new InMemoryGameRepository().Seed(game));

        Assert.Empty(await handler.HandleAsync(new GetPlayerLibraryQuery(PlayerId)));
    }

    [Fact]
    public async Task HandleAsync_ShouldListTheMostRecentAcquisitionFirst()
    {
        var older = Game.Create("Antigo", "Aventura", 10m);
        var newer = Game.Create("Recente", "Corrida", 10m);
        var handler = new GetPlayerLibraryHandler(
            new InMemoryLibraryItemRepository().Seed(
                LibraryItem.Create(PlayerId, older.Id, FixedNow.AddDays(-10), 10m),
                LibraryItem.Create(PlayerId, newer.Id, FixedNow, 10m)),
            new InMemoryGameRepository().Seed(older, newer));

        var summaries = await handler.HandleAsync(new GetPlayerLibraryQuery(PlayerId));

        Assert.Equal("Recente", summaries.First().GameName);
    }

    [Fact]
    public async Task HandleAsync_ShouldKeepGamesRemovedFromTheCatalog()
    {
        var removedGameId = Guid.NewGuid();
        var handler = new GetPlayerLibraryHandler(
            new InMemoryLibraryItemRepository()
                .Seed(LibraryItem.Create(PlayerId, removedGameId, FixedNow, 50m)),
            new InMemoryGameRepository());

        // A aquisição continua na biblioteca mesmo sem o jogo correspondente.
        var summary = Assert.Single(await handler.HandleAsync(new GetPlayerLibraryQuery(PlayerId)));

        Assert.Equal(removedGameId, summary.GameId);
        Assert.Equal(50m, summary.PricePaid);
    }
}
