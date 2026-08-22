using FCG.Domain.Common.Exceptions;
using FCG.Domain.Users;
using FCG.Domain.Users.Enums;
using FCG.Domain.Users.ValueObjects;

namespace FCG.Tests.Unit.Users;

/// <summary>
/// Valida as invariantes do agregado de usuário.
/// </summary>
public sealed class UserTests
{
    private const string ValidPasswordHash = "$2a$12$valid-password-hash";

    /// <summary>
    /// Garante que o cadastro público sempre origine um jogador ativo.
    /// </summary>
    [Fact]
    public void CreatePlayer_ShouldCreateActivePlayer()
    {
        var email = Email.Create("jogador@example.com");

        var user = User.CreatePlayer("Jogador", email, ValidPasswordHash);

        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal("Jogador", user.Name);
        Assert.Equal(email, user.Email);
        Assert.Equal(ValidPasswordHash, user.PasswordHash);
        Assert.Equal(UserRole.Player, user.Role);
        Assert.True(user.IsActive);
    }

    /// <summary>
    /// Garante que o fluxo administrativo possa criar uma conta administrativa distinta.
    /// </summary>
    [Fact]
    public void CreateAdministrator_ShouldCreateActiveAdministrator()
    {
        var user = User.CreateAdministrator(
            "Administrador",
            Email.Create("admin@example.com"),
            ValidPasswordHash);

        Assert.Equal(UserRole.Administrator, user.Role);
        Assert.True(user.IsActive);
    }

    /// <summary>
    /// Garante que o usuário não seja criado sem nome.
    /// </summary>
    [Fact]
    public void CreatePlayer_ShouldRejectEmptyName()
    {
        Assert.Throws<DomainException>(() => User.CreatePlayer(
            string.Empty,
            Email.Create("jogador@example.com"),
            ValidPasswordHash));
    }

    /// <summary>
    /// Garante que o usuário nunca armazene um hash de senha vazio.
    /// </summary>
    [Fact]
    public void CreatePlayer_ShouldRejectEmptyPasswordHash()
    {
        Assert.Throws<DomainException>(() => User.CreatePlayer(
            "Jogador",
            Email.Create("jogador@example.com"),
            string.Empty));
    }

    /// <summary>
    /// Garante a atualização dos dados mutáveis do perfil.
    /// </summary>
    [Fact]
    public void UpdateProfile_ShouldChangeNameAndEmail()
    {
        var user = User.CreatePlayer(
            "Nome original",
            Email.Create("original@example.com"),
            ValidPasswordHash);
        var updatedEmail = Email.Create("atualizado@example.com");

        user.UpdateProfile("Nome atualizado", updatedEmail);

        Assert.Equal("Nome atualizado", user.Name);
        Assert.Equal(updatedEmail, user.Email);
    }

    /// <summary>
    /// Garante que uma conta possa ser inativada e posteriormente reativada.
    /// </summary>
    [Fact]
    public void ActivationLifecycle_ShouldChangeActiveState()
    {
        var user = User.CreatePlayer(
            "Jogador",
            Email.Create("jogador@example.com"),
            ValidPasswordHash);

        user.Deactivate();
        Assert.False(user.IsActive);

        user.Activate();
        Assert.True(user.IsActive);
    }
}
