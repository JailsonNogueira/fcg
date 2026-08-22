using FCG.Domain.Common.Abstractions;
using FCG.Domain.Common.Exceptions;
using FCG.Domain.Users.Enums;
using FCG.Domain.Users.ValueObjects;

namespace FCG.Domain.Users;

/// <summary>
/// Representa uma conta de acesso da plataforma FCG.
/// </summary>
public sealed class User : IAggregateRoot
{
    private const int MaximumNameLength = 150;
    private const string InvalidNameMessage = "O nome do usuário deve ser informado.";
    private const string InvalidPasswordHashMessage = "O hash da senha deve ser informado.";

    private User()
    {
        Name = null!;
        Email = null!;
        PasswordHash = null!;
    }

    private User(string name, Email email, string passwordHash, UserRole role)
    {
        Id = Guid.NewGuid();
        Name = ValidateName(name);
        Email = email ?? throw new DomainException("O e-mail do usuário deve ser informado.");
        PasswordHash = ValidatePasswordHash(passwordHash);
        Role = role;
        IsActive = true;
    }

    /// <summary>
    /// Obtém o identificador único do usuário.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Obtém o nome de identificação do usuário.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Obtém o e-mail utilizado para autenticação.
    /// </summary>
    public Email Email { get; private set; }

    /// <summary>
    /// Obtém o hash seguro da senha do usuário.
    /// </summary>
    public string PasswordHash { get; private set; }

    /// <summary>
    /// Obtém o perfil de acesso imutável da conta.
    /// </summary>
    public UserRole Role { get; private set; }

    /// <summary>
    /// Indica se a conta está disponível para autenticação.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Cria uma conta de jogador para o fluxo público de cadastro.
    /// </summary>
    /// <param name="name">Nome de identificação do jogador.</param>
    /// <param name="email">E-mail validado do jogador.</param>
    /// <param name="passwordHash">Hash seguro da senha.</param>
    /// <returns>Conta ativa com perfil de jogador.</returns>
    public static User CreatePlayer(string name, Email email, string passwordHash)
    {
        return new User(name, email, passwordHash, UserRole.Player);
    }

    /// <summary>
    /// Cria uma conta administrativa para um fluxo previamente autorizado.
    /// </summary>
    /// <param name="name">Nome de identificação do administrador.</param>
    /// <param name="email">E-mail validado do administrador.</param>
    /// <param name="passwordHash">Hash seguro da senha.</param>
    /// <returns>Conta ativa com perfil administrativo.</returns>
    public static User CreateAdministrator(string name, Email email, string passwordHash)
    {
        return new User(name, email, passwordHash, UserRole.Administrator);
    }

    /// <summary>
    /// Atualiza os dados de identificação da conta.
    /// </summary>
    /// <param name="name">Novo nome de identificação.</param>
    /// <param name="email">Novo e-mail validado.</param>
    public void UpdateProfile(string name, Email email)
    {
        Name = ValidateName(name);
        Email = email ?? throw new DomainException("O e-mail do usuário deve ser informado.");
    }

    /// <summary>
    /// Substitui o hash após uma alteração segura de senha.
    /// </summary>
    /// <param name="passwordHash">Novo hash seguro da senha.</param>
    public void ChangePasswordHash(string passwordHash)
    {
        PasswordHash = ValidatePasswordHash(passwordHash);
    }

    /// <summary>
    /// Inativa a conta e impede novas autenticações.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Reativa uma conta previamente inativada.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }

    private static string ValidateName(string? name)
    {
        var normalizedName = name?.Trim();

        if (string.IsNullOrWhiteSpace(normalizedName) || normalizedName.Length > MaximumNameLength)
        {
            throw new DomainException(InvalidNameMessage);
        }

        return normalizedName;
    }

    private static string ValidatePasswordHash(string? passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new DomainException(InvalidPasswordHashMessage);
        }

        return passwordHash;
    }
}
