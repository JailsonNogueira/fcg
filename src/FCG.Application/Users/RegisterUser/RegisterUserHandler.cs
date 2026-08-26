using FCG.Application.Abstractions;
using FCG.Application.Common;
using FCG.Domain.Common.Exceptions;
using FCG.Domain.Users;
using FCG.Domain.Users.Enums;
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

        if (await users.ExistsByEmailAsync(email, cancellationToken: cancellationToken))
        {
            throw new ConflictException("Já existe um usuário cadastrado com este e-mail.");
        }

        var password = Password.Create(command.Password);
        var passwordHash = passwordHasher.Hash(password.Value);

        var user = command.Role switch
        {
            UserRole.Player => User.CreatePlayer(command.Name, email, passwordHash),
            UserRole.Administrator => User.CreateAdministrator(command.Name, email, passwordHash),
            _ => throw new DomainException("O perfil de acesso informado é inválido.")
        };

        await users.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
