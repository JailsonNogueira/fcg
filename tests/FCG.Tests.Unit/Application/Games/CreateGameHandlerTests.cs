using FCG.Application.Common;
using FCG.Application.Games.CreateGame;
using FCG.Domain.Games;
using FCG.Tests.Shared.Fakes;

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
        Assert.Equal("FIAP ADVENTURE", Assert.Single(repository.Items).NormalizedName);
        Assert.True(unitOfWork.WasSaved);
    }

    [Fact]
    public async Task HandleAsync_ShouldRejectDuplicateNormalizedName()
    {
        var repository = new InMemoryGameRepository()
            .Seed(Game.Create("FIAP Adventure", "Aventura", 99.90m));
        var handler = new CreateGameHandler(repository, new RecordingUnitOfWork());

        // Nome diferente apenas na caixa: a duplicidade é avaliada pelo nome normalizado.
        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.HandleAsync(new CreateGameCommand("fiap adventure", "Aventura", 99.90m)));
    }
}
