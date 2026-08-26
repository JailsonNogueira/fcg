namespace FCG.Application.Games.UpdateGame;

public sealed record UpdateGameCommand(Guid Id, string Name, string Description, decimal BasePrice);
