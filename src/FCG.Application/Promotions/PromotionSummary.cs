namespace FCG.Application.Promotions;

/// <summary>
/// Projeção de leitura de uma promoção.
/// </summary>
public sealed record PromotionSummary(
    Guid Id,
    Guid GameId,
    decimal DiscountPercentage,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    bool IsEnabled);
