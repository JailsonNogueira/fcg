using FCG.Api.Authorization;
using FCG.Application.Users.RegisterUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace FCG.Api.Controllers;

[ApiController, Route("api/users")]
public sealed class UsersController(RegisterUserHandler handler) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = Policies.ManageUsers)]
    public async Task<ActionResult> Create(RegisterUserCommand command, CancellationToken ct)
    {
        var id = await handler.HandleAsync(command, ct);
        return Created($"api/users/{id}", new { id });
    }
}
