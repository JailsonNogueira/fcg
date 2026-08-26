namespace FCG.Application.Games;

/// <summary>
/// Projeção de leitura de um jogo do catálogo.
/// </summary>
/// <param name="BasePrice">Preço original, sem promoção.</param>
/// <param name="CurrentPrice">Preço a ser cobrado agora, já com a promoção vigente aplicada.</param>
/// <param name="DiscountPercentage">Desconto vigente ou <see langword="null"/> quando não houver promoção.</param>
public sealed record GameSummary(
    Guid Id,
    string Name,
    string Description,
    decimal BasePrice,
    decimal CurrentPrice,
    decimal? DiscountPercentage,
    bool IsActive);
