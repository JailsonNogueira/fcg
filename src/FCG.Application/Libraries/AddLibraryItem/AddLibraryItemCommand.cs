namespace FCG.Application.Libraries.AddLibraryItem;

/// <summary>
/// Registra a aquisição de um jogo.
/// </summary>
/// <param name="PlayerId">
/// Jogador autenticado. Preenchido pela API a partir do token, nunca pelo corpo da requisição.
/// </param>
/// <param name="GameId">Jogo adquirido.</param>
public sealed record AddLibraryItemCommand(Guid PlayerId, Guid GameId);
