using FCG.Application.Abstractions;
using FCG.Application.Common;
using FCG.Domain.Users;
using FCG.Domain.Users.ValueObjects;

namespace FCG.Application.Users.UpdateUser;

public sealed class UpdateUserHandler(IUserRepository users, IUnitOfWork unitOfWork)
{
    public async Task HandleAsync(UpdateUserCommand command, CancellationToken cancellationToken = default)
    {
        var user = await users.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new KeyNotFoundException("O usuário informado não foi encontrado.");

        var email = Email.Create(command.Email);

        if (await users.ExistsByEmailAsync(email, command.Id, cancellationToken))
        {
            throw new ConflictException("Já existe um usuário cadastrado com este e-mail.");
        }

        user.UpdateProfile(command.Name, email);

        users.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
