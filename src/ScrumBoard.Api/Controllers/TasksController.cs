using Microsoft.AspNetCore.Mvc;
using ScrumBoard.Application.Tasks;

namespace ScrumBoard.Api.Controllers;

[ApiController]
public sealed class TasksController : ControllerBase
{
    private readonly TaskService _tasks;

    public TasksController(TaskService tasks) => _tasks = tasks;

    [HttpGet("api/tasks/{id:guid}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<TaskDto>> Get(Guid id, CancellationToken cancellationToken)
        => Ok(await _tasks.GetAsync(id, cancellationToken));

    [HttpGet("api/columns/{columnId:guid}/tasks")]
    [ProducesResponseType(typeof(IReadOnlyList<TaskDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TaskDto>>> ListByColumn(Guid columnId, CancellationToken cancellationToken)
        => Ok(await _tasks.ListByColumnAsync(columnId, cancellationToken));

    [HttpPost("api/tasks")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<TaskDto>> Create(CreateTaskRequest request, CancellationToken cancellationToken)
    {
        var created = await _tasks.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("api/tasks/{id:guid}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<TaskDto>> Update(Guid id, UpdateTaskRequest request, CancellationToken cancellationToken)
        => Ok(await _tasks.UpdateAsync(id, request, cancellationToken));

    [HttpDelete("api/tasks/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _tasks.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Drag and drop endpoint: moves the task to a column and places it between the two neighbours
    /// the client dropped it on. Returns 409 if those neighbours no longer describe a valid position.
    /// </summary>
    [HttpPatch("api/tasks/{id:guid}/position")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TaskDto>> Move(Guid id, MoveTaskRequest request, CancellationToken cancellationToken)
        => Ok(await _tasks.MoveAsync(id, request, cancellationToken));
}
