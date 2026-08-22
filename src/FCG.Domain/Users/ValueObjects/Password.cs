using FCG.Domain.Common.Exceptions;

namespace FCG.Domain.Users.ValueObjects;

/// <summary>
/// Representa uma senha em texto aberto validada para uso temporário no cadastro.
/// </summary>
/// <remarks>
/// Este objeto não deve ser persistido nem registrado em logs. A entidade de usuário
/// armazena somente o hash produzido pela infraestrutura de segurança.
/// </remarks>
public sealed class Password : IEquatable<Password>
{
    private const int MinimumLength = 8;
    private const string InvalidPasswordMessage =
        "A senha deve possuir pelo menos oito caracteres, uma letra, um número e um caractere especial.";
    private const string MaskedValue = "********";

    private Password(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Obtém temporariamente a senha validada para que ela seja convertida em hash.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Cria uma senha que atende aos critérios mínimos de segurança.
    /// </summary>
    /// <param name="value">Senha em texto aberto recebida no cadastro.</param>
    /// <returns>Objeto de valor que representa a senha validada.</returns>
    /// <exception cref="DomainException">Lançada quando a senha é insegura.</exception>
    public static Password Create(string? value)
    {
        if (string.IsNullOrEmpty(value)
            || value.Length < MinimumLength
            || value.Any(char.IsWhiteSpace)
            || !value.Any(char.IsLetter)
            || !value.Any(char.IsDigit)
            || !value.Any(character => !char.IsLetterOrDigit(character) && !char.IsWhiteSpace(character)))
        {
            throw new DomainException(InvalidPasswordMessage);
        }

        return new Password(value);
    }

    /// <inheritdoc />
    public bool Equals(Password? other)
    {
        return other is not null
            && string.Equals(Value, other.Value, StringComparison.Ordinal);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is Password other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return StringComparer.Ordinal.GetHashCode(Value);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return MaskedValue;
    }
}
