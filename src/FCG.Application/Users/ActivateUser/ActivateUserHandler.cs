using FCG.Application.Abstractions;
using FCG.Domain.Users;

namespace FCG.Application.Users.ActivateUser;

public sealed class ActivateUserHandler(IUserRepository users, IUnitOfWork unitOfWork)
{
    public async Task HandleAsync(ActivateUserCommand command, CancellationToken cancellationToken = default)
    {
        var user = await users.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new KeyNotFoundException("O usuário informado não foi encontrado.");

        if (user.IsActive)
        {
            return;
        }

        user.Activate();

        users.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
