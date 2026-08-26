using FCG.Domain.Users.Enums;

namespace FCG.Application.Users.RegisterUser;

/// <summary>
/// Cadastra uma conta na plataforma.
/// </summary>
/// <param name="Role">
/// Perfil da conta. O cadastro público sempre usa <see cref="UserRole.Player"/>;
/// apenas a área administrativa pode solicitar <see cref="UserRole.Administrator"/>.
/// </param>
public sealed record RegisterUserCommand(
    string Name,
    string Email,
    string Password,
    UserRole Role = UserRole.Player);
