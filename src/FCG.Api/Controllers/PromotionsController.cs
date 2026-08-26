using FCG.Api.Authorization;
using FCG.Application.Common;
using FCG.Application.Promotions;
using FCG.Application.Promotions.ActivatePromotion;
using FCG.Application.Promotions.CreatePromotion;
using FCG.Application.Promotions.DeactivatePromotion;
using FCG.Application.Promotions.GetPromotionById;
using FCG.Application.Promotions.GetPromotions;
using FCG.Application.Promotions.UpdatePromotion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Api.Controllers;

/// <summary>
/// Gestão de promoções. O jogador não consulta este recurso: o preço promocional já chega
/// aplicado nas respostas do catálogo em <c>api/games</c>.
/// </summary>
[ApiController]
[Route("api/promotions")]
[Authorize(Policy = Policies.ManagePromotions)]
public sealed class PromotionsController(
    CreatePromotionHandler createHandler,
    GetPromotionsHandler getPromotionsHandler,
    GetPromotionByIdHandler getPromotionByIdHandler,
    UpdatePromotionHandler updateHandler,
    DeactivatePromotionHandler deactivateHandler,
    ActivatePromotionHandler activateHandler) : ControllerBase
{
    /// <summary>Lista as promoções cadastradas.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<PromotionSummary>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<PromotionSummary>>> List(
        [FromQuery] Guid? gameId,
        [FromQuery] bool includeDisabled = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = PageRequest.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPromotionsQuery(gameId, includeDisabled, page, pageSize);

        return Ok(await getPromotionsHandler.HandleAsync(query, cancellationToken));
    }

    /// <summary>Obtém uma promoção pelo identificador.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PromotionSummary), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PromotionSummary>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await getPromotionByIdHandler.HandleAsync(new GetPromotionByIdQuery(id), cancellationToken));
    }

    /// <summary>Cadastra uma promoção para um jogo do catálogo.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Create(CreatePromotionCommand command, CancellationToken cancellationToken)
    {
        var id = await createHandler.HandleAsync(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    /// <summary>Atualiza o desconto e a vigência de uma promoção.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Update(
        Guid id,
        UpdatePromotionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdatePromotionCommand(
            id,
            request.DiscountPercentage,
            request.StartsAt,
            request.EndsAt);

        await updateHandler.HandleAsync(command, cancellationToken);

        return NoContent();
    }

    /// <summary>Desabilita a promoção, devolvendo o jogo ao preço-base.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        await deactivateHandler.HandleAsync(new DeactivatePromotionCommand(id), cancellationToken);

        return NoContent();
    }

    /// <summary>Reabilita uma promoção desabilitada.</summary>
    [HttpPost("{id:guid}/activation")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        await activateHandler.HandleAsync(new ActivatePromotionCommand(id), cancellationToken);

        return NoContent();
    }
}

public sealed record UpdatePromotionRequest(
    decimal DiscountPercentage,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt);
