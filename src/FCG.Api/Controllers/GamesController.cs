using FCG.Application.Games.CreateGame; using Microsoft.AspNetCore.Mvc;
namespace FCG.Api.Controllers;
[ApiController, Route("api/games")] public sealed class GamesController(CreateGameHandler handler) : ControllerBase { [HttpPost] public async Task<ActionResult> Create(CreateGameCommand command, CancellationToken ct) { var id = await handler.HandleAsync(command, ct); return Created($"api/games/{id}", new { id }); } }
