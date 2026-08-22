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
