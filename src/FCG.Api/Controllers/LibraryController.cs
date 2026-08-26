using FCG.Api.Authorization;
using FCG.Api.Extensions;
using FCG.Application.Libraries;
using FCG.Application.Libraries.AddLibraryItem;
using FCG.Application.Libraries.GetPlayerLibrary;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Api.Controllers;

/// <summary>
/// Biblioteca pessoal do jogador autenticado. O jogador e o preço vêm sempre do servidor,
/// nunca do corpo da requisição.
/// </summary>
[ApiController]
[Route("api/library")]
[Authorize(Policy = Policies.Library)]
public sealed class LibraryController(
    AddLibraryItemHandler addHandler,
    GetPlayerLibraryHandler getLibraryHandler) : ControllerBase
{
    /// <summary>Lista os jogos adquiridos pelo jogador autenticado.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<LibraryItemSummary>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<LibraryItemSummary>>> List(CancellationToken cancellationToken)
    {
        var query = new GetPlayerLibraryQuery(User.GetUserId());

        return Ok(await getLibraryHandler.HandleAsync(query, cancellationToken));
    }

    /// <summary>Adquire um jogo do catálogo pelo preço vigente.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Acquire(AcquireGameRequest request, CancellationToken cancellationToken)
    {
        var command = new AddLibraryItemCommand(User.GetUserId(), request.GameId);

        var id = await addHandler.HandleAsync(command, cancellationToken);

        return Created($"api/library/{id}", new { id });
    }
}

public sealed record AcquireGameRequest(Guid GameId);
