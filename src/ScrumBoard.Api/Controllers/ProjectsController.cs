using Microsoft.AspNetCore.Mvc;
using ScrumBoard.Application.Boards;
using ScrumBoard.Application.Common;
using ScrumBoard.Application.Projects;

namespace ScrumBoard.Api.Controllers;

[ApiController]
[Route("api/projects")]
public sealed class ProjectsController : ControllerBase
{
    private readonly ProjectService _projects;
    private readonly BoardColumnService _board;

    public ProjectsController(ProjectService projects, BoardColumnService board)
    {
        _projects = projects;
        _board = board;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProjectDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ProjectDto>>> Search(
        [FromQuery] string? name,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
        => Ok(await _projects.SearchAsync(new ProjectQuery(name, page, pageSize), cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectDto>> Get(Guid id, CancellationToken cancellationToken)
        => Ok(await _projects.GetAsync(id, cancellationToken));

    [HttpPost]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ProjectDto>> Create(SaveProjectRequest request, CancellationToken cancellationToken)
    {
        var created = await _projects.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProjectDto>> Update(Guid id, SaveProjectRequest request, CancellationToken cancellationToken)
        => Ok(await _projects.UpdateAsync(id, request, cancellationToken));

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _projects.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>Full board (columns with their ordered tasks) in a single request.</summary>
    [HttpGet("{id:guid}/board")]
    [ProducesResponseType(typeof(BoardDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<BoardDto>> Board(
        Guid id,
        [FromQuery] Guid? assigneeId,
        [FromQuery] int? priority,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
        => Ok(await _board.GetBoardAsync(id, new BoardFilter(assigneeId, priority, search), cancellationToken));
}
