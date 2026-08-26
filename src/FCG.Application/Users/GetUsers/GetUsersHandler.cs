using FCG.Application.Common;
using FCG.Domain.Users;

namespace FCG.Application.Users.GetUsers;

public sealed class GetUsersHandler(IUserRepository users)
{
    public async Task<PagedResult<UserSummary>> HandleAsync(
        GetUsersQuery query,
        CancellationToken cancellationToken = default)
    {
        var (page, pageSize, skip) = PageRequest.Normalize(query.Page, query.PageSize);

        var totalCount = await users.CountAsync(query.Role, query.IncludeInactive, cancellationToken);

        var items = totalCount == 0
            ? []
            : await users.SearchAsync(query.Role, query.IncludeInactive, skip, pageSize, cancellationToken);

        var summaries = items
            .Select(user => new UserSummary(
                user.Id,
                user.Name,
                user.Email.Value,
                user.Role.ToString(),
                user.IsActive))
            .ToList();

        return new PagedResult<UserSummary>(summaries, page, pageSize, totalCount);
    }
}
