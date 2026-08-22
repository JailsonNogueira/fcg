using FCG.Application.Abstractions;
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

        await promotions.AddAsync(promotion, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return promotion.Id;
    }
}
