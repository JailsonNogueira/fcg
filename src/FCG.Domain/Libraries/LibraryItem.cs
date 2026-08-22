using FCG.Domain.Common.Abstractions;
using FCG.Domain.Common.Exceptions;

namespace FCG.Domain.Libraries;

/// <summary>
/// Representa a posse de um jogo adquirido por um jogador.
/// </summary>
public sealed class LibraryItem : IAggregateRoot
{
    private const string InvalidPlayerMessage = "A aquisição deve estar associada a um jogador.";
    private const string InvalidGameMessage = "A aquisição deve estar associada a um jogo.";
    private const string InvalidAcquisitionDateMessage = "A data de aquisição deve ser informada.";
    private const string InvalidPriceMessage = "O preço pago não pode ser negativo.";

    private LibraryItem()
    {
    }

    private LibraryItem(
        Guid playerId,
        Guid gameId,
        DateTimeOffset acquiredAt,
        decimal pricePaid)
    {
        Id = Guid.NewGuid();
        PlayerId = ValidateIdentifier(playerId, InvalidPlayerMessage);
        GameId = ValidateIdentifier(gameId, InvalidGameMessage);
        AcquiredAt = ValidateAcquiredAt(acquiredAt);
        PricePaid = ValidatePricePaid(pricePaid);
    }

    /// <summary>
    /// Obtém o identificador único do item de biblioteca.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Obtém o jogador proprietário do jogo.
    /// </summary>
    public Guid PlayerId { get; private set; }

    /// <summary>
    /// Obtém o jogo adquirido.
    /// </summary>
    public Guid GameId { get; private set; }

    /// <summary>
    /// Obtém a data e hora em que a aquisição foi concluída.
    /// </summary>
    public DateTimeOffset AcquiredAt { get; private set; }

    /// <summary>
    /// Obtém o preço final registrado no momento da aquisição.
    /// </summary>
    public decimal PricePaid { get; private set; }

    /// <summary>
    /// Registra uma aquisição concluída na biblioteca de um jogador.
    /// </summary>
    /// <param name="playerId">Identificador do jogador.</param>
    /// <param name="gameId">Identificador do jogo.</param>
    /// <param name="acquiredAt">Data e hora da aquisição.</param>
    /// <param name="pricePaid">Preço final pago pelo jogo.</param>
    /// <returns>Novo item de biblioteca.</returns>
    public static LibraryItem Create(
        Guid playerId,
        Guid gameId,
        DateTimeOffset acquiredAt,
        decimal pricePaid)
    {
        return new LibraryItem(playerId, gameId, acquiredAt, pricePaid);
    }

    private static Guid ValidateIdentifier(Guid id, string errorMessage)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException(errorMessage);
        }

        return id;
    }

    private static DateTimeOffset ValidateAcquiredAt(DateTimeOffset acquiredAt)
    {
        if (acquiredAt == default)
        {
            throw new DomainException(InvalidAcquisitionDateMessage);
        }

        return acquiredAt;
    }

    private static decimal ValidatePricePaid(decimal pricePaid)
    {
        if (pricePaid < decimal.Zero)
        {
            throw new DomainException(InvalidPriceMessage);
        }

        return pricePaid;
    }
}
