using FCG.Domain.Games;
using FCG.Domain.Promotions;

namespace FCG.Application.Games;

/// <summary>
/// Converte um jogo em projeção de catálogo aplicando a promoção vigente, quando houver.
/// </summary>
internal static class GameCatalogMapper
{
    public static GameSummary ToSummary(Game game, Promotion? promotion, DateTimeOffset referenceTime)
    {
        if (promotion is null || !promotion.IsActiveAt(referenceTime))
        {
            return new GameSummary(
                game.Id,
                game.Name,
                game.Description,
                game.BasePrice,
                game.BasePrice,
                DiscountPercentage: null,
                game.IsActive);
        }

        return new GameSummary(
            game.Id,
            game.Name,
            game.Description,
            game.BasePrice,
            promotion.ApplyTo(game.BasePrice, referenceTime),
            promotion.DiscountPercentage,
            game.IsActive);
    }
}
