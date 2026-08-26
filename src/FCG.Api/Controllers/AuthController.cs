using FCG.Application.Users.AuthenticateUser;
using FCG.Application.Users.RegisterUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Api.Controllers;

/// <summary>
/// Porta de entrada pública da plataforma: qualquer pessoa cria a própria conta de jogador
/// e autentica. A gestão de contas pela equipe administrativa fica em <c>api/users</c>.
/// </summary>
[ApiController]
[Route("auth")]
[AllowAnonymous]
public sealed class AuthController(
    RegisterUserHandler registerHandler,
    AuthenticateUserHandler authenticateHandler) : ControllerBase
{
    /// <summary>Cadastra uma nova conta de jogador.</summary>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Register(
        [FromBody] RegisterPlayerRequest request,
        CancellationToken cancellationToken)
    {
        // O perfil não vem do corpo: o cadastro público sempre cria um jogador.
        var command = new RegisterUserCommand(request.Name, request.Email, request.Password);

        var id = await registerHandler.HandleAsync(command, cancellationToken);

        return Created($"api/users/{id}", new { id });
    }

    /// <summary>Autentica uma conta e devolve o token de acesso.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthenticationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthenticationResult>> Login(
        [FromBody] AuthenticateUserCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await authenticateHandler.HandleAsync(command, cancellationToken));
    }
}

public sealed record RegisterPlayerRequest(string Name, string Email, string Password);
