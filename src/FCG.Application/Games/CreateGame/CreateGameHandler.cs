using FCG.Application.Abstractions;
using FCG.Application.Common;
using FCG.Domain.Games;

namespace FCG.Application.Games.CreateGame;

public sealed class CreateGameHandler(IGameRepository games, IUnitOfWork unitOfWork)
{
    public async Task<Guid> HandleAsync(CreateGameCommand command, CancellationToken cancellationToken = default)
    {
        var game = Game.Create(command.Name, command.Description, command.BasePrice);

        if (await games.ExistsByNormalizedNameAsync(game.NormalizedName, cancellationToken: cancellationToken))
        {
            throw new ConflictException("Já existe um jogo cadastrado com este nome.");
        }

        await games.AddAsync(game, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return game.Id;
    }
}
