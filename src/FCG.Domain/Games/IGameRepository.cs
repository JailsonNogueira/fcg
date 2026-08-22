namespace FCG.Domain.Games;

/// <summary>
/// Define as operações de persistência necessárias para o agregado de jogo.
/// </summary>
public interface IGameRepository
{
    /// <summary>
    /// Obtém um jogo pelo identificador.
    /// </summary>
    /// <param name="id">Identificador do jogo.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>Jogo encontrado ou <see langword="null"/>.</returns>
    Task<Game?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se o nome normalizado já pertence a outro jogo.
    /// </summary>
    /// <param name="normalizedName">Nome normalizado do jogo.</param>
    /// <param name="ignoredGameId">Jogo desconsiderado durante uma atualização.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns><see langword="true"/> quando houver duplicidade.</returns>
    Task<bool> ExistsByNormalizedNameAsync(
        string normalizedName,
        Guid? ignoredGameId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adiciona um novo jogo ao repositório.
    /// </summary>
    /// <param name="game">Jogo que será persistido.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    Task AddAsync(Game game, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marca um jogo existente para atualização.
    /// </summary>
    /// <param name="game">Jogo alterado.</param>
    void Update(Game game);
}
