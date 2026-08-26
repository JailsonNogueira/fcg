using FCG.Application.Common;

namespace FCG.Application.Promotions.GetPromotions;

public sealed record GetPromotionsQuery(
    Guid? GameId = null,
    bool IncludeDisabled = false,
    int Page = 1,
    int PageSize = PageRequest.DefaultPageSize);
