namespace FCG.Application.Libraries;

/// <summary>
/// Projeção de leitura de um jogo pertencente à biblioteca de um jogador.
/// </summary>
public sealed record LibraryItemSummary(
    Guid Id,
    Guid GameId,
    string GameName,
    string GameDescription,
    DateTimeOffset AcquiredAt,
    decimal PricePaid);
