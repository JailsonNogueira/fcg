using FCG.Api.Authorization;
using FCG.Application.Games.CreateGame;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace FCG.Api.Controllers;

[ApiController, Route("api/games")]
public sealed class GamesController(CreateGameHandler handler) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = Policies.ManageCatalog)]
    public async Task<ActionResult> Create(CreateGameCommand command, CancellationToken ct)
    {
        var id = await handler.HandleAsync(command, ct);
        return Created($"api/games/{id}", new { id });
    }
}
