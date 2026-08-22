using FCG.Application.Users.RegisterUser; using Microsoft.AspNetCore.Mvc;
namespace FCG.Api.Controllers;
[ApiController, Route("api/users")] public sealed class UsersController(RegisterUserHandler handler) : ControllerBase { [HttpPost] public async Task<ActionResult> Create(RegisterUserCommand command, CancellationToken ct) { var id = await handler.HandleAsync(command, ct); return Created($"api/users/{id}", new { id }); } }
