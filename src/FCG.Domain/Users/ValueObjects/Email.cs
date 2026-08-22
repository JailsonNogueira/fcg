using System.Text.RegularExpressions;

using FCG.Domain.Common.Exceptions;

namespace FCG.Domain.Users.ValueObjects;

/// <summary>
/// Representa um endereço de e-mail válido e normalizado.
/// </summary>
public sealed class Email : IEquatable<Email>
{
    private const int MaximumLength = 254;
    private const string InvalidEmailMessage = "O e-mail informado possui formato inválido.";
    private const string ValidEmailExpression = "^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$";

    private static readonly Regex ValidEmailPattern = new(
        ValidEmailExpression,
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private Email(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Obtém o endereço de e-mail normalizado.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Cria um endereço de e-mail validado e normalizado.
    /// </summary>
    /// <param name="value">Endereço de e-mail informado pelo usuário.</param>
    /// <returns>Objeto de valor que representa o e-mail.</returns>
    /// <exception cref="DomainException">Lançada quando o e-mail é inválido.</exception>
    public static Email Create(string? value)
    {
        var normalizedValue = value?.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(normalizedValue)
            || normalizedValue.Length > MaximumLength
            || !ValidEmailPattern.IsMatch(normalizedValue))
        {
            throw new DomainException(InvalidEmailMessage);
        }

        return new Email(normalizedValue);
    }

    /// <inheritdoc />
    public bool Equals(Email? other)
    {
        return other is not null
            && string.Equals(Value, other.Value, StringComparison.Ordinal);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is Email other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return StringComparer.Ordinal.GetHashCode(Value);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Value;
    }
}
