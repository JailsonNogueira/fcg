namespace FCG.Application.Promotions.CreatePromotion;

public sealed record CreatePromotionCommand(
    Guid GameId,
    decimal DiscountPercentage,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt);
