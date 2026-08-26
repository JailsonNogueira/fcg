using FCG.Application.Abstractions;
using FCG.Domain.Promotions;

namespace FCG.Application.Promotions.DeactivatePromotion;

public sealed class DeactivatePromotionHandler(IPromotionRepository promotions, IUnitOfWork unitOfWork)
{
    public async Task HandleAsync(DeactivatePromotionCommand command, CancellationToken cancellationToken = default)
    {
        var promotion = await promotions.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new KeyNotFoundException("A promoção informada não foi encontrada.");

        if (!promotion.IsEnabled)
        {
            return;
        }

        promotion.Deactivate();

        promotions.Update(promotion);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
