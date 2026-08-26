using FCG.Application.Abstractions;
using FCG.Domain.Games;
using FCG.Domain.Promotions;

namespace FCG.Application.Games.GetGameById;

public sealed class GetGameByIdHandler(
    IGameRepository games,
    IPromotionRepository promotions,
    IClock clock)
{
    public async Task<GameSummary> HandleAsync(
        GetGameByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var game = await games.GetByIdAsync(query.Id, cancellationToken)
            ?? throw new KeyNotFoundException("O jogo informado não foi encontrado.");

        var referenceTime = clock.UtcNow;
        var promotion = await promotions.GetActiveByGameIdAsync(game.Id, referenceTime, cancellationToken);

        return GameCatalogMapper.ToSummary(game, promotion, referenceTime);
    }
}
