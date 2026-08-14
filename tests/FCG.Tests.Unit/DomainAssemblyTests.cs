using FCG.Domain.Entities;

namespace FCG.Tests.Unit;

public sealed class DomainAssemblyTests
{
    [Fact]
    public void DomainAssemblyShouldBeAvailable()
    {
        Assert.Equal("FCG.Domain", FCG.Domain.DomainAssembly.Reference.GetName().Name);
    }

    // Smoke test: confirms domain entities are reachable from the assembly.
    // Business-rule tests for User belong to the dedicated unit-test task (Backlog Onda 2).
    [Fact]
    public void UserEntityShouldBeInstantiableFromDomainAssembly()
    {
        var user = new User("Alice", "alice@test.com", "secret", "Jogador");

        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal("Alice", user.Name);
        Assert.Equal("alice@test.com", user.Email);
        Assert.NotNull(user.Password);
        Assert.Equal("Jogador", user.Role);
    }
}
