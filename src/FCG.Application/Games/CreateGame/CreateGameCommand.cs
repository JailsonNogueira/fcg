namespace FCG.Application.Games.CreateGame;

public sealed record CreateGameCommand(string Name, string Description, decimal BasePrice);
