using FCG.Application.Common;

namespace FCG.Application.Games.GetGames;

public sealed record GetGamesQuery(
    string? Search = null,
    bool IncludeInactive = false,
    int Page = 1,
    int PageSize = PageRequest.DefaultPageSize);
