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
    /// Obtém os jogos correspondentes a um conjunto de identificadores.
    /// </summary>
    /// <param name="ids">Identificadores procurados.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>Jogos encontrados, em quantidade menor ou igual à de identificadores.</returns>
    Task<IReadOnlyCollection<Game>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default);

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
    /// Obtém uma página do catálogo ordenada por nome.
    /// </summary>
    /// <param name="term">Trecho do nome usado como filtro opcional.</param>
    /// <param name="includeInactive">Indica se jogos inativos entram no resultado.</param>
    /// <param name="skip">Quantidade de registros ignorados.</param>
    /// <param name="take">Quantidade máxima de registros retornados.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>Jogos encontrados na página solicitada.</returns>
    Task<IReadOnlyCollection<Game>> SearchAsync(
        string? term,
        bool includeInactive,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Conta os jogos que atendem aos mesmos filtros de <see cref="SearchAsync"/>.
    /// </summary>
    /// <param name="term">Trecho do nome usado como filtro opcional.</param>
    /// <param name="includeInactive">Indica se jogos inativos entram na contagem.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>Quantidade total de jogos filtrados.</returns>
    Task<int> CountAsync(
        string? term,
        bool includeInactive,
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
