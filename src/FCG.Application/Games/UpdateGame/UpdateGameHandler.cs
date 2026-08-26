using FCG.Application.Abstractions;
using FCG.Application.Common;
using FCG.Domain.Games;

namespace FCG.Application.Games.UpdateGame;

public sealed class UpdateGameHandler(IGameRepository games, IUnitOfWork unitOfWork)
{
    public async Task HandleAsync(UpdateGameCommand command, CancellationToken cancellationToken = default)
    {
        var game = await games.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new KeyNotFoundException("O jogo informado não foi encontrado.");

        game.UpdateDetails(command.Name, command.Description, command.BasePrice);

        if (await games.ExistsByNormalizedNameAsync(game.NormalizedName, command.Id, cancellationToken))
        {
            throw new ConflictException("Já existe um jogo cadastrado com este nome.");
        }

        games.Update(game);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
