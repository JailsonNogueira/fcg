using FCG.Application.Common;
using FCG.Domain.Users.Enums;

namespace FCG.Application.Users.GetUsers;

public sealed record GetUsersQuery(
    UserRole? Role = null,
    bool IncludeInactive = false,
    int Page = 1,
    int PageSize = PageRequest.DefaultPageSize);
