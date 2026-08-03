using Microsoft.AspNetCore.Mvc;
using ScrumBoard.Application.Boards;

namespace ScrumBoard.Api.Controllers;

[ApiController]
public sealed class ColumnsController : ControllerBase
{
    private readonly BoardColumnService _columns;

    public ColumnsController(BoardColumnService columns) => _columns = columns;

    [HttpGet("api/projects/{projectId:guid}/columns")]
    [ProducesResponseType(typeof(IReadOnlyList<BoardColumnDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<BoardColumnDto>>> List(Guid projectId, CancellationToken cancellationToken)
        => Ok(await _columns.ListAsync(projectId, cancellationToken));

    [HttpPost("api/projects/{projectId:guid}/columns")]
    [ProducesResponseType(typeof(BoardColumnDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<BoardColumnDto>> Create(Guid projectId, SaveColumnRequest request, CancellationToken cancellationToken)
    {
        var created = await _columns.CreateAsync(projectId, request, cancellationToken);
        return CreatedAtAction(nameof(List), new { projectId }, created);
    }

    [HttpPut("api/columns/{id:guid}")]
    [ProducesResponseType(typeof(BoardColumnDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<BoardColumnDto>> Update(Guid id, SaveColumnRequest request, CancellationToken cancellationToken)
        => Ok(await _columns.UpdateAsync(id, request, cancellationToken));

    /// <summary>Returns 409 when the column still holds tasks (business rule 6.4).</summary>
    [HttpDelete("api/columns/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _columns.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPut("api/projects/{projectId:guid}/columns/order")]
    [ProducesResponseType(typeof(IReadOnlyList<BoardColumnDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<BoardColumnDto>>> Reorder(
        Guid projectId,
        ReorderColumnsRequest request,
        CancellationToken cancellationToken)
        => Ok(await _columns.ReorderAsync(projectId, request, cancellationToken));
}
