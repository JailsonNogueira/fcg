using FCG.Api.Authorization;
using FCG.Application.Promotions.CreatePromotion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace FCG.Api.Controllers;

[ApiController, Route("api/promotions")]
public sealed class PromotionsController(CreatePromotionHandler handler) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = Policies.ManagePromotions)]
    public async Task<ActionResult> Create(CreatePromotionCommand command, CancellationToken ct)
    {
        var id = await handler.HandleAsync(command, ct);
        return Created($"api/promotions/{id}", new { id });
    }
}
