using FCG.Domain.Common.Exceptions;
using FCG.Domain.Games;

namespace FCG.Tests.Unit.Games;

/// <summary>
/// Valida as invariantes do agregado de jogo.
/// </summary>
public sealed class GameTests
{
    /// <summary>
    /// Garante a criação de um jogo ativo com nome normalizado para unicidade.
    /// </summary>
    [Fact]
    public void Create_ShouldCreateActiveGame()
    {
        var game = Game.Create("  FIAP Adventure  ", "Jogo de aventura", 99.90m);

        Assert.NotEqual(Guid.Empty, game.Id);
        Assert.Equal("FIAP Adventure", game.Name);
        Assert.Equal("FIAP ADVENTURE", game.NormalizedName);
        Assert.Equal("Jogo de aventura", game.Description);
        Assert.Equal(99.90m, game.BasePrice);
        Assert.True(game.IsActive);
    }

    /// <summary>
    /// Garante que jogos gratuitos sejam aceitos pelo catálogo.
    /// </summary>
    [Fact]
    public void Create_ShouldAcceptFreeGame()
    {
        var game = Game.Create("Jogo gratuito", "Descrição", decimal.Zero);

        Assert.Equal(decimal.Zero, game.BasePrice);
    }

    /// <summary>
    /// Garante que dados obrigatórios e preços negativos sejam rejeitados.
    /// </summary>
    /// <param name="name">Nome do jogo.</param>
    /// <param name="description">Descrição do jogo.</param>
    /// <param name="basePrice">Preço-base do jogo.</param>
    [Theory]
    [InlineData("", "Descrição", 10)]
    [InlineData("Jogo", "", 10)]
    [InlineData("Jogo", "Descrição", -1)]
    public void Create_ShouldRejectInvalidGame(string name, string description, decimal basePrice)
    {
        Assert.Throws<DomainException>(() => Game.Create(name, description, basePrice));
    }

    /// <summary>
    /// Garante que a atualização preserve a normalização usada contra duplicidades.
    /// </summary>
    [Fact]
    public void UpdateDetails_ShouldUpdateCatalogData()
    {
        var game = Game.Create("Nome original", "Descrição original", 10m);

        game.UpdateDetails("  Novo nome  ", "Nova descrição", 20m);

        Assert.Equal("Novo nome", game.Name);
        Assert.Equal("NOVO NOME", game.NormalizedName);
        Assert.Equal("Nova descrição", game.Description);
        Assert.Equal(20m, game.BasePrice);
    }

    /// <summary>
    /// Garante a retirada lógica do catálogo sem apagar o jogo.
    /// </summary>
    [Fact]
    public void AvailabilityLifecycle_ShouldChangeActiveState()
    {
        var game = Game.Create("Jogo", "Descrição", 10m);

        game.Deactivate();
        Assert.False(game.IsActive);

        game.Activate();
        Assert.True(game.IsActive);
    }
}
