using FCG.Application.Abstractions;
using FCG.Domain.Games;

namespace FCG.Application.Games.ActivateGame;

public sealed class ActivateGameHandler(IGameRepository games, IUnitOfWork unitOfWork)
{
    public async Task HandleAsync(ActivateGameCommand command, CancellationToken cancellationToken = default)
    {
        var game = await games.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new KeyNotFoundException("O jogo informado não foi encontrado.");

        if (game.IsActive)
        {
            return;
        }

        game.Activate();

        games.Update(game);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
