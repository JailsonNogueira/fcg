using FCG.Domain.Common.Exceptions;
using FCG.Domain.Promotions;

namespace FCG.Tests.Unit.Promotions;

/// <summary>
/// Valida as invariantes do agregado de promoção.
/// </summary>
public sealed class PromotionTests
{
    private static readonly DateTimeOffset StartsAt = new(2026, 8, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset EndsAt = new(2026, 8, 31, 23, 59, 59, TimeSpan.Zero);

    /// <summary>
    /// Garante a criação de uma promoção habilitada com vigência válida.
    /// </summary>
    [Fact]
    public void Create_ShouldCreateEnabledPromotion()
    {
        var gameId = Guid.NewGuid();

        var promotion = Promotion.Create(gameId, 15m, StartsAt, EndsAt);

        Assert.NotEqual(Guid.Empty, promotion.Id);
        Assert.Equal(gameId, promotion.GameId);
        Assert.Equal(15m, promotion.DiscountPercentage);
        Assert.True(promotion.IsEnabled);
    }

    /// <summary>
    /// Garante que o percentual permaneça no intervalo permitido.
    /// </summary>
    /// <param name="percentage">Percentual inválido.</param>
    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(100.01)]
    public void Create_ShouldRejectInvalidDiscountPercentage(decimal percentage)
    {
        Assert.Throws<DomainException>(() => Promotion.Create(
            Guid.NewGuid(),
            percentage,
            StartsAt,
            EndsAt));
    }

    /// <summary>
    /// Garante que o término da promoção seja posterior ao início.
    /// </summary>
    [Fact]
    public void Create_ShouldRejectInvalidPeriod()
    {
        Assert.Throws<DomainException>(() => Promotion.Create(
            Guid.NewGuid(),
            10m,
            EndsAt,
            StartsAt));
    }

    /// <summary>
    /// Garante que a promoção esteja ativa apenas durante a vigência.
    /// </summary>
    [Fact]
    public void IsActiveAt_ShouldRespectValidityPeriod()
    {
        var promotion = Promotion.Create(Guid.NewGuid(), 10m, StartsAt, EndsAt);

        Assert.False(promotion.IsActiveAt(StartsAt.AddTicks(-1)));
        Assert.True(promotion.IsActiveAt(StartsAt));
        Assert.True(promotion.IsActiveAt(EndsAt));
        Assert.False(promotion.IsActiveAt(EndsAt.AddTicks(1)));
    }

    /// <summary>
    /// Garante o cálculo do preço promocional sem alterar o preço-base do jogo.
    /// </summary>
    [Fact]
    public void ApplyTo_ShouldCalculateDiscountedPrice()
    {
        var promotion = Promotion.Create(Guid.NewGuid(), 15m, StartsAt, EndsAt);

        var promotionalPrice = promotion.ApplyTo(99.90m, StartsAt);

        Assert.Equal(84.92m, promotionalPrice);
    }

    /// <summary>
    /// Garante que uma promoção desabilitada não seja aplicada.
    /// </summary>
    [Fact]
    public void ApplyTo_ShouldRejectDisabledPromotion()
    {
        var promotion = Promotion.Create(Guid.NewGuid(), 15m, StartsAt, EndsAt);
        promotion.Deactivate();

        Assert.Throws<DomainException>(() => promotion.ApplyTo(100m, StartsAt));
    }
}
