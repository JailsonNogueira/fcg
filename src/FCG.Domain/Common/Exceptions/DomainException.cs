namespace FCG.Domain.Common.Exceptions;

/// <summary>
/// Representa a violação de uma regra de negócio do domínio.
/// </summary>
public sealed class DomainException : Exception
{
    /// <summary>
    /// Inicializa uma nova exceção de domínio.
    /// </summary>
    /// <param name="message">Mensagem que descreve a regra violada.</param>
    public DomainException(string message)
        : base(message)
    {
    }
}
