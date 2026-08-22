using FCG.Domain.Common.Abstractions;
using FCG.Domain.Common.Exceptions;

namespace FCG.Domain.Promotions;

/// <summary>
/// Representa um desconto temporário aplicado a um jogo específico.
/// </summary>
public sealed class Promotion : IAggregateRoot
{
    private const decimal MinimumDiscountPercentage = 0m;
    private const decimal MaximumDiscountPercentage = 100m;
    private const decimal PercentageDivisor = 100m;
    private const int CurrencyDecimalPlaces = 2;
    private const string InvalidGameMessage = "A promoção deve estar associada a um jogo.";
    private const string InvalidPercentageMessage = "O percentual de desconto deve ser maior que zero e menor ou igual a cem.";
    private const string InvalidPeriodMessage = "O término da promoção deve ser posterior ao início.";
    private const string InvalidBasePriceMessage = "O preço-base não pode ser negativo.";
    private const string InactivePromotionMessage = "A promoção não está ativa na data informada.";

    private Promotion()
    {
    }

    private Promotion(
        Guid gameId,
        decimal discountPercentage,
        DateTimeOffset startsAt,
        DateTimeOffset endsAt)
    {
        Id = Guid.NewGuid();
        GameId = ValidateGameId(gameId);
        SetDetails(discountPercentage, startsAt, endsAt);
        IsEnabled = true;
    }

    /// <summary>
    /// Obtém o identificador único da promoção.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Obtém o jogo beneficiado pela promoção.
    /// </summary>
    public Guid GameId { get; private set; }

    /// <summary>
    /// Obtém o percentual de desconto aplicado sobre o preço-base.
    /// </summary>
    public decimal DiscountPercentage { get; private set; }

    /// <summary>
    /// Obtém o início inclusivo da vigência.
    /// </summary>
    public DateTimeOffset StartsAt { get; private set; }

    /// <summary>
    /// Obtém o término inclusivo da vigência.
    /// </summary>
    public DateTimeOffset EndsAt { get; private set; }

    /// <summary>
    /// Indica se a promoção está habilitada para avaliação de vigência.
    /// </summary>
    public bool IsEnabled { get; private set; }

    /// <summary>
    /// Cria uma promoção habilitada para um jogo.
    /// </summary>
    /// <param name="gameId">Identificador do jogo.</param>
    /// <param name="discountPercentage">Percentual de desconto.</param>
    /// <param name="startsAt">Início inclusivo da vigência.</param>
    /// <param name="endsAt">Término inclusivo da vigência.</param>
    /// <returns>Novo agregado de promoção.</returns>
    public static Promotion Create(
        Guid gameId,
        decimal discountPercentage,
        DateTimeOffset startsAt,
        DateTimeOffset endsAt)
    {
        return new Promotion(gameId, discountPercentage, startsAt, endsAt);
    }

    /// <summary>
    /// Atualiza o desconto e o período de vigência.
    /// </summary>
    /// <param name="discountPercentage">Novo percentual de desconto.</param>
    /// <param name="startsAt">Novo início inclusivo da vigência.</param>
    /// <param name="endsAt">Novo término inclusivo da vigência.</param>
    public void UpdateDetails(
        decimal discountPercentage,
        DateTimeOffset startsAt,
        DateTimeOffset endsAt)
    {
        SetDetails(discountPercentage, startsAt, endsAt);
    }

    /// <summary>
    /// Verifica se a promoção pode ser aplicada em uma data de referência.
    /// </summary>
    /// <param name="referenceTime">Data e hora usadas na avaliação.</param>
    /// <returns><see langword="true"/> quando a promoção estiver habilitada e vigente.</returns>
    public bool IsActiveAt(DateTimeOffset referenceTime)
    {
        return IsEnabled && referenceTime >= StartsAt && referenceTime <= EndsAt;
    }

    /// <summary>
    /// Calcula o preço promocional sem modificar o preço-base do jogo.
    /// </summary>
    /// <param name="basePrice">Preço original do jogo.</param>
    /// <param name="referenceTime">Data e hora usadas na avaliação da vigência.</param>
    /// <returns>Preço final arredondado para duas casas decimais.</returns>
    public decimal ApplyTo(decimal basePrice, DateTimeOffset referenceTime)
    {
        if (basePrice < decimal.Zero)
        {
            throw new DomainException(InvalidBasePriceMessage);
        }

        if (!IsActiveAt(referenceTime))
        {
            throw new DomainException(InactivePromotionMessage);
        }

        var discountMultiplier = DiscountPercentage / PercentageDivisor;
        var promotionalPrice = basePrice * (decimal.One - discountMultiplier);

        return decimal.Round(promotionalPrice, CurrencyDecimalPlaces, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Desabilita a promoção antes ou depois de sua vigência.
    /// </summary>
    public void Deactivate()
    {
        IsEnabled = false;
    }

    /// <summary>
    /// Habilita novamente a promoção para avaliação de vigência.
    /// </summary>
    public void Activate()
    {
        IsEnabled = true;
    }

    private static Guid ValidateGameId(Guid gameId)
    {
        if (gameId == Guid.Empty)
        {
            throw new DomainException(InvalidGameMessage);
        }

        return gameId;
    }

    private void SetDetails(
        decimal discountPercentage,
        DateTimeOffset startsAt,
        DateTimeOffset endsAt)
    {
        if (discountPercentage <= MinimumDiscountPercentage
            || discountPercentage > MaximumDiscountPercentage)
        {
            throw new DomainException(InvalidPercentageMessage);
        }

        if (endsAt <= startsAt)
        {
            throw new DomainException(InvalidPeriodMessage);
        }

        DiscountPercentage = discountPercentage;
        StartsAt = startsAt;
        EndsAt = endsAt;
    }
}
