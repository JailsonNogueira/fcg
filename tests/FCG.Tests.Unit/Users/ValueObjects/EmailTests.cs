using FCG.Domain.Common.Exceptions;
using FCG.Domain.Users.ValueObjects;

namespace FCG.Tests.Unit.Users.ValueObjects;

/// <summary>
/// Valida as regras do objeto de valor de e-mail.
/// </summary>
public sealed class EmailTests
{
    /// <summary>
    /// Garante que um endereço válido seja normalizado antes de ser armazenado.
    /// </summary>
    [Fact]
    public void Create_ShouldNormalizeValidEmail()
    {
        var email = Email.Create("  Jogador@Example.COM  ");

        Assert.Equal("jogador@example.com", email.Value);
    }

    /// <summary>
    /// Garante que formatos inválidos sejam rejeitados pelo domínio.
    /// </summary>
    /// <param name="value">Valor de e-mail inválido.</param>
    [Theory]
    [InlineData("")]
    [InlineData("jogador")]
    [InlineData("jogador@")]
    [InlineData("@example.com")]
    [InlineData("jogador@example")]
    [InlineData("jogador @example.com")]
    public void Create_ShouldRejectInvalidEmail(string value)
    {
        Assert.Throws<DomainException>(() => Email.Create(value));
    }

    /// <summary>
    /// Garante que e-mails equivalentes possuam igualdade por valor.
    /// </summary>
    [Fact]
    public void Equals_ShouldUseNormalizedValue()
    {
        var first = Email.Create("JOGADOR@example.com");
        var second = Email.Create("jogador@EXAMPLE.COM");

        Assert.Equal(first, second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }
}
