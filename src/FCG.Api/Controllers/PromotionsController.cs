using FCG.Application.Promotions.CreatePromotion; using Microsoft.AspNetCore.Mvc;
namespace FCG.Api.Controllers;
[ApiController, Route("api/promotions")] public sealed class PromotionsController(CreatePromotionHandler handler) : ControllerBase { [HttpPost] public async Task<ActionResult> Create(CreatePromotionCommand command, CancellationToken ct) { var id = await handler.HandleAsync(command, ct); return Created($"api/promotions/{id}", new { id }); } }
