using FCG.Application.Common;
using FCG.Domain.Promotions;

namespace FCG.Application.Promotions.GetPromotions;

public sealed class GetPromotionsHandler(IPromotionRepository promotions)
{
    public async Task<PagedResult<PromotionSummary>> HandleAsync(
        GetPromotionsQuery query,
        CancellationToken cancellationToken = default)
    {
        var (page, pageSize, skip) = PageRequest.Normalize(query.Page, query.PageSize);

        var totalCount = await promotions.CountAsync(query.GameId, query.IncludeDisabled, cancellationToken);

        var items = totalCount == 0
            ? []
            : await promotions.SearchAsync(query.GameId, query.IncludeDisabled, skip, pageSize, cancellationToken);

        var summaries = items
            .Select(promotion => new PromotionSummary(
                promotion.Id,
                promotion.GameId,
                promotion.DiscountPercentage,
                promotion.StartsAt,
                promotion.EndsAt,
                promotion.IsEnabled))
            .ToList();

        return new PagedResult<PromotionSummary>(summaries, page, pageSize, totalCount);
    }
}
