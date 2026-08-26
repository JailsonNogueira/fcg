namespace FCG.Application.Promotions.UpdatePromotion;

public sealed record UpdatePromotionCommand(
    Guid Id,
    decimal DiscountPercentage,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt);
