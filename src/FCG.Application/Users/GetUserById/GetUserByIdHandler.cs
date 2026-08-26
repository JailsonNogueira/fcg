using FCG.Domain.Users;

namespace FCG.Application.Users.GetUserById;

public sealed class GetUserByIdHandler(IUserRepository users)
{
    public async Task<UserSummary> HandleAsync(
        GetUserByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var user = await users.GetByIdAsync(query.Id, cancellationToken)
            ?? throw new KeyNotFoundException("O usuário informado não foi encontrado.");

        return new UserSummary(
            user.Id,
            user.Name,
            user.Email.Value,
            user.Role.ToString(),
            user.IsActive);
    }
}
