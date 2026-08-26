using FCG.Api.Authorization;
using FCG.Application.Common;
using FCG.Application.Games;
using FCG.Application.Games.ActivateGame;
using FCG.Application.Games.CreateGame;
using FCG.Application.Games.DeactivateGame;
using FCG.Application.Games.GetGameById;
using FCG.Application.Games.GetGames;
using FCG.Application.Games.UpdateGame;
using FCG.Domain.Users.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Api.Controllers;

/// <summary>
/// Catálogo de jogos: leitura aberta a qualquer conta autenticada, manutenção restrita
/// aos administradores.
/// </summary>
[ApiController]
[Route("api/games")]
public sealed class GamesController(
    CreateGameHandler createHandler,
    GetGamesHandler getGamesHandler,
    GetGameByIdHandler getGameByIdHandler,
    UpdateGameHandler updateHandler,
    DeactivateGameHandler deactivateHandler,
    ActivateGameHandler activateHandler) : ControllerBase
{
    /// <summary>Consulta o catálogo já com o preço promocional vigente.</summary>
    [HttpGet]
    [Authorize(Policy = Policies.Catalog)]
    [ProducesResponseType(typeof(PagedResult<GameSummary>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<GameSummary>>> List(
        [FromQuery] string? search,
        [FromQuery] bool includeInactive = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = PageRequest.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        // Jogos fora do catálogo só aparecem para quem os administra.
        var listInactive = includeInactive && User.IsInRole(nameof(UserRole.Administrator));
        var query = new GetGamesQuery(search, listInactive, page, pageSize);

        return Ok(await getGamesHandler.HandleAsync(query, cancellationToken));
    }

    /// <summary>Obtém um jogo pelo identificador.</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = Policies.Catalog)]
    [ProducesResponseType(typeof(GameSummary), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GameSummary>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await getGameByIdHandler.HandleAsync(new GetGameByIdQuery(id), cancellationToken));
    }

    /// <summary>Cadastra um jogo no catálogo.</summary>
    [HttpPost]
    [Authorize(Policy = Policies.ManageCatalog)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Create(CreateGameCommand command, CancellationToken cancellationToken)
    {
        var id = await createHandler.HandleAsync(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    /// <summary>Atualiza os dados comerciais de um jogo.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = Policies.ManageCatalog)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Update(
        Guid id,
        UpdateGameRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateGameCommand(id, request.Name, request.Description, request.BasePrice);

        await updateHandler.HandleAsync(command, cancellationToken);

        return NoContent();
    }

    /// <summary>Retira o jogo do catálogo sem apagar as bibliotecas de quem já o adquiriu.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policies.ManageCatalog)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        await deactivateHandler.HandleAsync(new DeactivateGameCommand(id), cancellationToken);

        return NoContent();
    }

    /// <summary>Disponibiliza novamente um jogo retirado do catálogo.</summary>
    [HttpPost("{id:guid}/activation")]
    [Authorize(Policy = Policies.ManageCatalog)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        await activateHandler.HandleAsync(new ActivateGameCommand(id), cancellationToken);

        return NoContent();
    }
}

public sealed record UpdateGameRequest(string Name, string Description, decimal BasePrice);
