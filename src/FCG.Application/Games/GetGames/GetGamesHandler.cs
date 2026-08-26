using FCG.Application.Abstractions;
using FCG.Application.Common;
using FCG.Domain.Games;
using FCG.Domain.Promotions;

namespace FCG.Application.Games.GetGames;

public sealed class GetGamesHandler(
    IGameRepository games,
    IPromotionRepository promotions,
    IClock clock)
{
    public async Task<PagedResult<GameSummary>> HandleAsync(
        GetGamesQuery query,
        CancellationToken cancellationToken = default)
    {
        var (page, pageSize, skip) = PageRequest.Normalize(query.Page, query.PageSize);

        var totalCount = await games.CountAsync(query.Search, query.IncludeInactive, cancellationToken);

        if (totalCount == 0)
        {
            return new PagedResult<GameSummary>([], page, pageSize, totalCount);
        }

        var items = await games.SearchAsync(query.Search, query.IncludeInactive, skip, pageSize, cancellationToken);

        if (items.Count == 0)
        {
            return new PagedResult<GameSummary>([], page, pageSize, totalCount);
        }

        var referenceTime = clock.UtcNow;
        var gameIds = items.Select(game => game.Id).ToList();

        // Uma única consulta de promoções para a página inteira evita N+1.
        var activePromotions = (await promotions.GetActiveByGameIdsAsync(gameIds, referenceTime, cancellationToken))
            .GroupBy(promotion => promotion.GameId)
            .ToDictionary(group => group.Key, group => group.First());

        var summaries = items
            .Select(game => GameCatalogMapper.ToSummary(
                game,
                activePromotions.GetValueOrDefault(game.Id),
                referenceTime))
            .ToList();

        return new PagedResult<GameSummary>(summaries, page, pageSize, totalCount);
    }
}
