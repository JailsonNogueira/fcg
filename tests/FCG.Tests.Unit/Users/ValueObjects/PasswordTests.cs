using FCG.Domain.Common.Exceptions;
using FCG.Domain.Users.ValueObjects;

namespace FCG.Tests.Unit.Users.ValueObjects;

/// <summary>
/// Valida os critérios de segurança do objeto de valor de senha.
/// </summary>
public sealed class PasswordTests
{
    /// <summary>
    /// Garante a criação de uma senha que atende a todos os critérios obrigatórios.
    /// </summary>
    [Fact]
    public void Create_ShouldAcceptStrongPassword()
    {
        var password = Password.Create("Senha@123");

        Assert.Equal("Senha@123", password.Value);
    }

    /// <summary>
    /// Garante a rejeição de senhas que não atendem aos critérios mínimos.
    /// </summary>
    /// <param name="value">Senha insegura.</param>
    [Theory]
    [InlineData("")]
    [InlineData("Ab@123")]
    [InlineData("12345678@")]
    [InlineData("SomenteLetras@")]
    [InlineData("Senha1234")]
    public void Create_ShouldRejectWeakPassword(string value)
    {
        Assert.Throws<DomainException>(() => Password.Create(value));
    }

    /// <summary>
    /// Garante que a senha não seja exposta por conversão textual acidental.
    /// </summary>
    [Fact]
    public void ToString_ShouldMaskPassword()
    {
        var password = Password.Create("Senha@123");

        Assert.Equal("********", password.ToString());
    }
}
