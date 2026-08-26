using FCG.Application.Abstractions;
using FCG.Application.Common;
using FCG.Domain.Promotions;

namespace FCG.Application.Promotions.UpdatePromotion;

public sealed class UpdatePromotionHandler(IPromotionRepository promotions, IUnitOfWork unitOfWork)
{
    public async Task HandleAsync(UpdatePromotionCommand command, CancellationToken cancellationToken = default)
    {
        var promotion = await promotions.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new KeyNotFoundException("A promoção informada não foi encontrada.");

        promotion.UpdateDetails(command.DiscountPercentage, command.StartsAt, command.EndsAt);

        if (await promotions.ExistsOverlappingAsync(
                promotion.GameId,
                promotion.StartsAt,
                promotion.EndsAt,
                promotion.Id,
                cancellationToken))
        {
            throw new ConflictException("O jogo já possui uma promoção vigente no período informado.");
        }

        promotions.Update(promotion);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
