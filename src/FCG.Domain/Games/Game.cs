using FCG.Domain.Common.Abstractions;
using FCG.Domain.Common.Exceptions;

namespace FCG.Domain.Games;

/// <summary>
/// Representa um jogo disponível no catálogo da plataforma.
/// </summary>
public sealed class Game : IAggregateRoot
{
    private const int MaximumNameLength = 200;
    private const int MaximumDescriptionLength = 2000;
    private const string InvalidNameMessage = "O nome do jogo deve ser informado.";
    private const string InvalidDescriptionMessage = "A descrição do jogo deve ser informada.";
    private const string InvalidPriceMessage = "O preço-base do jogo não pode ser negativo.";

    private Game()
    {
    }

    private Game(string name, string description, decimal basePrice)
    {
        Id = Guid.NewGuid();
        SetDetails(name, description, basePrice);
        IsActive = true;
    }

    /// <summary>
    /// Obtém o identificador único do jogo.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Obtém o nome de exibição do jogo.
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Obtém o nome normalizado utilizado na verificação de duplicidade do catálogo.
    /// </summary>
    public string NormalizedName { get; private set; } = null!;

    /// <summary>
    /// Obtém a descrição comercial do jogo.
    /// </summary>
    public string Description { get; private set; } = null!;

    /// <summary>
    /// Obtém o preço original, sem aplicação de promoção.
    /// </summary>
    public decimal BasePrice { get; private set; }

    /// <summary>
    /// Indica se o jogo pode ser encontrado e adquirido no catálogo.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Cria um jogo ativo para inclusão no catálogo.
    /// </summary>
    /// <param name="name">Nome do jogo.</param>
    /// <param name="description">Descrição do jogo.</param>
    /// <param name="basePrice">Preço original do jogo.</param>
    /// <returns>Novo agregado de jogo.</returns>
    public static Game Create(string name, string description, decimal basePrice)
    {
        return new Game(name, description, basePrice);
    }

    /// <summary>
    /// Atualiza os dados comerciais do jogo.
    /// </summary>
    /// <param name="name">Novo nome do jogo.</param>
    /// <param name="description">Nova descrição do jogo.</param>
    /// <param name="basePrice">Novo preço original do jogo.</param>
    public void UpdateDetails(string name, string description, decimal basePrice)
    {
        SetDetails(name, description, basePrice);
    }

    /// <summary>
    /// Retira o jogo do catálogo sem apagar aquisições existentes.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Disponibiliza novamente o jogo no catálogo.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }

    private void SetDetails(string? name, string? description, decimal basePrice)
    {
        var normalizedDisplayName = name?.Trim();
        var normalizedDescription = description?.Trim();

        if (string.IsNullOrWhiteSpace(normalizedDisplayName)
            || normalizedDisplayName.Length > MaximumNameLength)
        {
            throw new DomainException(InvalidNameMessage);
        }

        if (string.IsNullOrWhiteSpace(normalizedDescription)
            || normalizedDescription.Length > MaximumDescriptionLength)
        {
            throw new DomainException(InvalidDescriptionMessage);
        }

        if (basePrice < decimal.Zero)
        {
            throw new DomainException(InvalidPriceMessage);
        }

        Name = normalizedDisplayName;
        NormalizedName = normalizedDisplayName.ToUpperInvariant();
        Description = normalizedDescription;
        BasePrice = basePrice;
    }
}
