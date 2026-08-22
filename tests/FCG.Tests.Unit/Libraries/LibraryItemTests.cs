using FCG.Domain.Common.Exceptions;
using FCG.Domain.Libraries;

namespace FCG.Tests.Unit.Libraries;

/// <summary>
/// Valida as invariantes do agregado que representa um jogo adquirido.
/// </summary>
public sealed class LibraryItemTests
{
    /// <summary>
    /// Garante o registro imutável da aquisição de um jogo pelo jogador.
    /// </summary>
    [Fact]
    public void Create_ShouldRegisterGameAcquisition()
    {
        var playerId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var acquiredAt = DateTimeOffset.UtcNow;

        var item = LibraryItem.Create(playerId, gameId, acquiredAt, 84.92m);

        Assert.NotEqual(Guid.Empty, item.Id);
        Assert.Equal(playerId, item.PlayerId);
        Assert.Equal(gameId, item.GameId);
        Assert.Equal(acquiredAt, item.AcquiredAt);
        Assert.Equal(84.92m, item.PricePaid);
    }

    /// <summary>
    /// Garante que a aquisição sempre esteja associada a um jogador.
    /// </summary>
    [Fact]
    public void Create_ShouldRejectEmptyPlayerId()
    {
        Assert.Throws<DomainException>(() => LibraryItem.Create(
            Guid.Empty,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            10m));
    }

    /// <summary>
    /// Garante que a aquisição sempre esteja associada a um jogo.
    /// </summary>
    [Fact]
    public void Create_ShouldRejectEmptyGameId()
    {
        Assert.Throws<DomainException>(() => LibraryItem.Create(
            Guid.NewGuid(),
            Guid.Empty,
            DateTimeOffset.UtcNow,
            10m));
    }

    /// <summary>
    /// Garante que o preço efetivamente pago nunca seja negativo.
    /// </summary>
    [Fact]
    public void Create_ShouldRejectNegativePricePaid()
    {
        Assert.Throws<DomainException>(() => LibraryItem.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            -1m));
    }
}
