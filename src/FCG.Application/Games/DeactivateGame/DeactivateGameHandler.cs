using FCG.Application.Abstractions;
using FCG.Domain.Games;

namespace FCG.Application.Games.DeactivateGame;

public sealed class DeactivateGameHandler(IGameRepository games, IUnitOfWork unitOfWork)
{
    public async Task HandleAsync(DeactivateGameCommand command, CancellationToken cancellationToken = default)
    {
        var game = await games.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new KeyNotFoundException("O jogo informado não foi encontrado.");

        if (!game.IsActive)
        {
            return;
        }

        game.Deactivate();

        games.Update(game);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
