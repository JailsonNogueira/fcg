using FCG.Domain.Promotions;

namespace FCG.Application.Promotions.GetPromotionById;

public sealed class GetPromotionByIdHandler(IPromotionRepository promotions)
{
    public async Task<PromotionSummary> HandleAsync(
        GetPromotionByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var promotion = await promotions.GetByIdAsync(query.Id, cancellationToken)
            ?? throw new KeyNotFoundException("A promoção informada não foi encontrada.");

        return new PromotionSummary(
            promotion.Id,
            promotion.GameId,
            promotion.DiscountPercentage,
            promotion.StartsAt,
            promotion.EndsAt,
            promotion.IsEnabled);
    }
}
