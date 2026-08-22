namespace FCG.Domain.Libraries;

/// <summary>
/// Define as operações de persistência necessárias para a biblioteca do jogador.
/// </summary>
public interface ILibraryItemRepository
{
    /// <summary>
    /// Verifica se um jogador já possui determinado jogo.
    /// </summary>
    /// <param name="playerId">Identificador do jogador.</param>
    /// <param name="gameId">Identificador do jogo.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns><see langword="true"/> quando o jogo já pertencer à biblioteca.</returns>
    Task<bool> ExistsAsync(
        Guid playerId,
        Guid gameId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém todos os jogos pertencentes a um jogador.
    /// </summary>
    /// <param name="playerId">Identificador do jogador.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>Itens existentes na biblioteca.</returns>
    Task<IReadOnlyCollection<LibraryItem>> GetByPlayerIdAsync(
        Guid playerId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adiciona uma aquisição à biblioteca do jogador.
    /// </summary>
    /// <param name="libraryItem">Item de biblioteca que será persistido.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    Task AddAsync(LibraryItem libraryItem, CancellationToken cancellationToken = default);
}
