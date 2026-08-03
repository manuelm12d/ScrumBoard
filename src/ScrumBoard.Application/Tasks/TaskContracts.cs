using ScrumBoard.Domain.Tasks;

namespace ScrumBoard.Application.Tasks;

public sealed record TaskDto(
    Guid Id,
    Guid ColumnId,
    Guid ProjectId,
    string Title,
    string? Description,
    TaskPriority Priority,
    Guid? AssigneeId,
    string? AssigneeName,
    decimal Position,
    DateTime CreatedAtUtc);

public sealed record CreateTaskRequest(
    Guid ColumnId,
    string Title,
    string? Description,
    TaskPriority Priority,
    Guid? AssigneeId);

public sealed record UpdateTaskRequest(
    string Title,
    string? Description,
    TaskPriority Priority,
    Guid? AssigneeId);

/// <summary>
/// Drag and drop payload. The client sends the neighbours the task was dropped between instead of a
/// numeric index, which keeps the request valid even if another session reordered the column meanwhile.
/// </summary>
public sealed record MoveTaskRequest(Guid TargetColumnId, Guid? PreviousTaskId, Guid? NextTaskId);

public sealed record TaskMovedEvent(Guid TaskId, Guid ProjectId, Guid FromColumnId, Guid ToColumnId, decimal Position);
