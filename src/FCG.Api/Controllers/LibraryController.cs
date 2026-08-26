using FCG.Api.Authorization;
using FCG.Application.Libraries.AddLibraryItem;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace FCG.Api.Controllers;
[ApiController, Route("api/library")]
public sealed class LibraryController(AddLibraryItemHandler handler) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = Policies.Library)]
    public async Task<ActionResult> Add(AddLibraryItemCommand command, CancellationToken ct)
    {
        var id = await handler.HandleAsync(command, ct);
        return Created($"api/library/{id}", new { id });
    }
}
