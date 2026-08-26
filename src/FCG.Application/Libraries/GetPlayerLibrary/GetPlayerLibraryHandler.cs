using FCG.Domain.Games;
using FCG.Domain.Libraries;

namespace FCG.Application.Libraries.GetPlayerLibrary;

public sealed class GetPlayerLibraryHandler(
    ILibraryItemRepository libraryItems,
    IGameRepository games)
{
    private const string MissingGameName = "Jogo indisponível";

    public async Task<IReadOnlyCollection<LibraryItemSummary>> HandleAsync(
        GetPlayerLibraryQuery query,
        CancellationToken cancellationToken = default)
    {
        var items = await libraryItems.GetByPlayerIdAsync(query.PlayerId, cancellationToken);

        if (items.Count == 0)
        {
            return [];
        }

        var gameIds = items.Select(item => item.GameId).Distinct().ToList();
        var ownedGames = (await games.GetByIdsAsync(gameIds, cancellationToken))
            .ToDictionary(game => game.Id);

        return items
            .OrderByDescending(item => item.AcquiredAt)
            .Select(item =>
            {
                // Um jogo retirado do catálogo continua na biblioteca de quem já comprou.
                var game = ownedGames.GetValueOrDefault(item.GameId);

                return new LibraryItemSummary(
                    item.Id,
                    item.GameId,
                    game?.Name ?? MissingGameName,
                    game?.Description ?? string.Empty,
                    item.AcquiredAt,
                    item.PricePaid);
            })
            .ToList();
    }
}
