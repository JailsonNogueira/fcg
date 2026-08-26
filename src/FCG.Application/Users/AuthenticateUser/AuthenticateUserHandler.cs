using FCG.Application.Abstractions;
using FCG.Application.Common;
using FCG.Domain.Common.Exceptions;
using FCG.Domain.Users;
using FCG.Domain.Users.ValueObjects;

namespace FCG.Application.Users.AuthenticateUser;

public sealed class AuthenticateUserHandler(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    ITokenGenerator tokenGenerator)
{
    private const string InvalidCredentialsMessage = "Credenciais inválidas.";

    public async Task<AuthenticationResult> HandleAsync(
        AuthenticateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        Email email;

        try
        {
            email = Email.Create(command.Email);
        }
        catch (DomainException)
        {
            // Um e-mail malformado não revela se a conta existe: responde como credencial inválida.
            throw new UnauthorizedException(InvalidCredentialsMessage);
        }

        var user = await users.GetByEmailAsync(email, cancellationToken);

        if (user is null
            || !user.IsActive
            || !passwordHasher.Verify(command.Password ?? string.Empty, user.PasswordHash))
        {
            throw new UnauthorizedException(InvalidCredentialsMessage);
        }

        return new AuthenticationResult(
            tokenGenerator.Generate(user),
            user.Id,
            user.Name,
            user.Email.Value,
            user.Role.ToString());
    }
}
