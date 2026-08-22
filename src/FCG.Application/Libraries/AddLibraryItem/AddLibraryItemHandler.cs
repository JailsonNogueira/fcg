using FCG.Application.Abstractions;
using FCG.Application.Common;
using FCG.Domain.Games;
using FCG.Domain.Libraries;
using FCG.Domain.Users;

namespace FCG.Application.Libraries.AddLibraryItem;

public sealed class AddLibraryItemHandler(
    IUserRepository users,
    IGameRepository games,
    ILibraryItemRepository libraryItems,
    IClock clock,
    IUnitOfWork unitOfWork)
{
    public async Task<Guid> HandleAsync(AddLibraryItemCommand command, CancellationToken cancellationToken = default)
    {
        if (await users.GetByIdAsync(command.PlayerId, cancellationToken) is null)
        {
            throw new KeyNotFoundException("O usuário informado não foi encontrado.");
        }

        if (await games.GetByIdAsync(command.GameId, cancellationToken) is null)
        {
            throw new KeyNotFoundException("O jogo informado não foi encontrado.");
        }

        if (await libraryItems.ExistsAsync(command.PlayerId, command.GameId, cancellationToken))
        {
            throw new ConflictException("O usuário já possui este jogo em sua biblioteca.");
        }

        var item = LibraryItem.Create(command.PlayerId, command.GameId, clock.UtcNow, command.PricePaid);

        await libraryItems.AddAsync(item, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return item.Id;
    }
}
