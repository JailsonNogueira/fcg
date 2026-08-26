using FCG.Application.Abstractions;
using FCG.Application.Common;
using FCG.Domain.Games;
using FCG.Domain.Libraries;
using FCG.Domain.Promotions;
using FCG.Domain.Users;

namespace FCG.Application.Libraries.AddLibraryItem;

public sealed class AddLibraryItemHandler(
    IUserRepository users,
    IGameRepository games,
    IPromotionRepository promotions,
    ILibraryItemRepository libraryItems,
    IClock clock,
    IUnitOfWork unitOfWork)
{
    public async Task<Guid> HandleAsync(AddLibraryItemCommand command, CancellationToken cancellationToken = default)
    {
        var user = await users.GetByIdAsync(command.PlayerId, cancellationToken)
            ?? throw new KeyNotFoundException("O usuário informado não foi encontrado.");

        if (!user.IsActive)
        {
            throw new ConflictException("A conta informada está inativa.");
        }

        var game = await games.GetByIdAsync(command.GameId, cancellationToken)
            ?? throw new KeyNotFoundException("O jogo informado não foi encontrado.");

        if (!game.IsActive)
        {
            throw new ConflictException("O jogo informado não está disponível no catálogo.");
        }

        if (await libraryItems.ExistsAsync(command.PlayerId, command.GameId, cancellationToken))
        {
            throw new ConflictException("O usuário já possui este jogo em sua biblioteca.");
        }

        var acquiredAt = clock.UtcNow;

        // O preço é sempre resolvido no servidor: preço-base do catálogo com a promoção vigente.
        var promotion = await promotions.GetActiveByGameIdAsync(game.Id, acquiredAt, cancellationToken);
        var pricePaid = promotion is null
            ? game.BasePrice
            : promotion.ApplyTo(game.BasePrice, acquiredAt);

        var item = LibraryItem.Create(command.PlayerId, command.GameId, acquiredAt, pricePaid);

        await libraryItems.AddAsync(item, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return item.Id;
    }
}
