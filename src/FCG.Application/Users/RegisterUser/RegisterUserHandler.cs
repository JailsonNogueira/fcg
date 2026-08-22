using FCG.Application.Abstractions;
using FCG.Application.Common;
using FCG.Domain.Users;
using FCG.Domain.Users.ValueObjects;

namespace FCG.Application.Users.RegisterUser;

public sealed class RegisterUserHandler(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork)
{
    public async Task<Guid> HandleAsync(RegisterUserCommand command, CancellationToken cancellationToken = default)
    {
        var email = Email.Create(command.Email);

        if (await users.ExistsByEmailAsync(email, cancellationToken))
        {
            throw new ConflictException("Já existe um usuário cadastrado com este e-mail.");
        }

        var password = Password.Create(command.Password);
        var user = User.CreatePlayer(command.Name, email, passwordHasher.Hash(password.Value));

        await users.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
