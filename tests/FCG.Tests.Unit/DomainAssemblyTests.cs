using FCG.Domain.Users;
using FCG.Domain.Users.Enums;
using FCG.Domain.Users.ValueObjects;

namespace FCG.Tests.Unit;

/// <summary>
/// Valida a disponibilidade da assembly e dos tipos públicos do domínio.
/// </summary>
public sealed class DomainAssemblyTests
{
    /// <summary>
    /// Garante que a referência da assembly de domínio esteja disponível.
    /// </summary>
    [Fact]
    public void DomainAssemblyShouldBeAvailable()
    {
        Assert.Equal("FCG.Domain", FCG.Domain.DomainAssembly.Reference.GetName().Name);
    }

    /// <summary>
    /// Garante que os agregados possam ser acessados pela assembly de testes.
    /// </summary>
    [Fact]
    public void UserEntityShouldBeInstantiableFromDomainAssembly()
    {
        var email = Email.Create("alice@test.com");
        var user = User.CreatePlayer("Alice", email, "password-hash");

        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal("Alice", user.Name);
        Assert.Equal(email, user.Email);
        Assert.NotNull(user.PasswordHash);
        Assert.Equal(UserRole.Player, user.Role);
    }
}
