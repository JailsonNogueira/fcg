using FCG.Application.Abstractions;
using FCG.Application.Common;
using FCG.Domain.Games;
using FCG.Domain.Promotions;

namespace FCG.Application.Promotions.CreatePromotion;

public sealed class CreatePromotionHandler(
    IGameRepository games,
    IPromotionRepository promotions,
    IUnitOfWork unitOfWork)
{
    public async Task<Guid> HandleAsync(CreatePromotionCommand command, CancellationToken cancellationToken = default)
    {
        if (await games.GetByIdAsync(command.GameId, cancellationToken) is null)
        {
            throw new KeyNotFoundException("O jogo informado não foi encontrado.");
        }

        var promotion = Promotion.Create(
            command.GameId,
            command.DiscountPercentage,
            command.StartsAt,
            command.EndsAt);

        // Duas promoções vigentes ao mesmo tempo tornariam o preço do catálogo ambíguo.
        if (await promotions.ExistsOverlappingAsync(
                promotion.GameId,
                promotion.StartsAt,
                promotion.EndsAt,
                cancellationToken: cancellationToken))
        {
            throw new ConflictException("O jogo já possui uma promoção vigente no período informado.");
        }

        await promotions.AddAsync(promotion, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return promotion.Id;
    }
}
