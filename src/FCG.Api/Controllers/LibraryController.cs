using FCG.Application.Libraries.AddLibraryItem; using Microsoft.AspNetCore.Mvc;
namespace FCG.Api.Controllers;
[ApiController, Route("api/library")] public sealed class LibraryController(AddLibraryItemHandler handler) : ControllerBase { [HttpPost] public async Task<ActionResult> Add(AddLibraryItemCommand command, CancellationToken ct) { var id = await handler.HandleAsync(command, ct); return Created($"api/library/{id}", new { id }); } }
