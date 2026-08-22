namespace FCG.Application.Libraries.AddLibraryItem;

public sealed record AddLibraryItemCommand(Guid PlayerId, Guid GameId, decimal PricePaid);
