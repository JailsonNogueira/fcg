namespace FCG.Domain.Promotions;

/// <summary>
/// Define as operações de persistência necessárias para o agregado de promoção.
/// </summary>
public interface IPromotionRepository
{
    /// <summary>
    /// Obtém uma promoção pelo identificador.
    /// </summary>
    /// <param name="id">Identificador da promoção.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>Promoção encontrada ou <see langword="null"/>.</returns>
    Task<Promotion?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém a promoção ativa de um jogo em uma data de referência.
    /// </summary>
    /// <param name="gameId">Identificador do jogo.</param>
    /// <param name="referenceTime">Data e hora usadas na consulta.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>Promoção aplicável ou <see langword="null"/>.</returns>
    Task<Promotion?> GetActiveByGameIdAsync(
        Guid gameId,
        DateTimeOffset referenceTime,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém as promoções ativas de um conjunto de jogos em uma data de referência.
    /// </summary>
    /// <param name="gameIds">Identificadores dos jogos consultados.</param>
    /// <param name="referenceTime">Data e hora usadas na consulta.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>Promoções aplicáveis, no máximo uma por jogo.</returns>
    Task<IReadOnlyCollection<Promotion>> GetActiveByGameIdsAsync(
        IReadOnlyCollection<Guid> gameIds,
        DateTimeOffset referenceTime,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se o jogo já possui uma promoção habilitada no período informado.
    /// </summary>
    /// <param name="gameId">Identificador do jogo.</param>
    /// <param name="startsAt">Início inclusivo do período avaliado.</param>
    /// <param name="endsAt">Término inclusivo do período avaliado.</param>
    /// <param name="ignoredPromotionId">Promoção desconsiderada durante uma atualização.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns><see langword="true"/> quando houver sobreposição de vigência.</returns>
    Task<bool> ExistsOverlappingAsync(
        Guid gameId,
        DateTimeOffset startsAt,
        DateTimeOffset endsAt,
        Guid? ignoredPromotionId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém uma página de promoções ordenada pelo início da vigência.
    /// </summary>
    /// <param name="gameId">Jogo usado como filtro opcional.</param>
    /// <param name="includeDisabled">Indica se promoções desabilitadas entram no resultado.</param>
    /// <param name="skip">Quantidade de registros ignorados.</param>
    /// <param name="take">Quantidade máxima de registros retornados.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>Promoções encontradas na página solicitada.</returns>
    Task<IReadOnlyCollection<Promotion>> SearchAsync(
        Guid? gameId,
        bool includeDisabled,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Conta as promoções que atendem aos mesmos filtros de <see cref="SearchAsync"/>.
    /// </summary>
    /// <param name="gameId">Jogo usado como filtro opcional.</param>
    /// <param name="includeDisabled">Indica se promoções desabilitadas entram na contagem.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>Quantidade total de promoções filtradas.</returns>
    Task<int> CountAsync(
        Guid? gameId,
        bool includeDisabled,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adiciona uma nova promoção ao repositório.
    /// </summary>
    /// <param name="promotion">Promoção que será persistida.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    Task AddAsync(Promotion promotion, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marca uma promoção existente para atualização.
    /// </summary>
    /// <param name="promotion">Promoção alterada.</param>
    void Update(Promotion promotion);
}
