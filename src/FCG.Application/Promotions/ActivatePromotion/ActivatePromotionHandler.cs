using FCG.Application.Abstractions;
using FCG.Application.Common;
using FCG.Domain.Promotions;

namespace FCG.Application.Promotions.ActivatePromotion;

public sealed class ActivatePromotionHandler(IPromotionRepository promotions, IUnitOfWork unitOfWork)
{
    public async Task HandleAsync(ActivatePromotionCommand command, CancellationToken cancellationToken = default)
    {
        var promotion = await promotions.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new KeyNotFoundException("A promoção informada não foi encontrada.");

        if (promotion.IsEnabled)
        {
            return;
        }

        // Reabilitar não pode ressuscitar uma sobreposição criada enquanto a promoção estava desligada.
        if (await promotions.ExistsOverlappingAsync(
                promotion.GameId,
                promotion.StartsAt,
                promotion.EndsAt,
                promotion.Id,
                cancellationToken))
        {
            throw new ConflictException("O jogo já possui uma promoção vigente no período informado.");
        }

        promotion.Activate();

        promotions.Update(promotion);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
