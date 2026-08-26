using FCG.Api.Authorization;
using FCG.Application.Common;
using FCG.Application.Users;
using FCG.Application.Users.ActivateUser;
using FCG.Application.Users.DeactivateUser;
using FCG.Application.Users.GetUserById;
using FCG.Application.Users.GetUsers;
using FCG.Application.Users.RegisterUser;
using FCG.Application.Users.UpdateUser;
using FCG.Domain.Users.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Api.Controllers;

/// <summary>
/// Área administrativa de contas: gere os usuários já existentes na plataforma e cria
/// novas contas, inclusive administrativas. O cadastro aberto ao público fica em <c>auth/register</c>.
/// </summary>
[ApiController]
[Route("api/users")]
[Authorize(Policy = Policies.ManageUsers)]
public sealed class UsersController(
    RegisterUserHandler registerHandler,
    GetUsersHandler getUsersHandler,
    GetUserByIdHandler getUserByIdHandler,
    UpdateUserHandler updateHandler,
    DeactivateUserHandler deactivateHandler,
    ActivateUserHandler activateHandler) : ControllerBase
{
    /// <summary>Lista as contas da plataforma.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<UserSummary>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<UserSummary>>> List(
        [FromQuery] UserRole? role,
        [FromQuery] bool includeInactive = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = PageRequest.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var query = new GetUsersQuery(role, includeInactive, page, pageSize);

        return Ok(await getUsersHandler.HandleAsync(query, cancellationToken));
    }

    /// <summary>Obtém uma conta pelo identificador.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserSummary), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserSummary>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await getUserByIdHandler.HandleAsync(new GetUserByIdQuery(id), cancellationToken));
    }

    /// <summary>Cria uma conta de jogador ou de administrador.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Create(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var id = await registerHandler.HandleAsync(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    /// <summary>Atualiza o nome e o e-mail de uma conta.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Update(
        Guid id,
        UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateUserCommand(id, request.Name, request.Email);

        await updateHandler.HandleAsync(command, cancellationToken);

        return NoContent();
    }

    /// <summary>Inativa uma conta, preservando seu histórico na plataforma.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        await deactivateHandler.HandleAsync(new DeactivateUserCommand(id), cancellationToken);

        return NoContent();
    }

    /// <summary>Reativa uma conta previamente inativada.</summary>
    [HttpPost("{id:guid}/activation")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        await activateHandler.HandleAsync(new ActivateUserCommand(id), cancellationToken);

        return NoContent();
    }
}

public sealed record UpdateUserRequest(string Name, string Email);
