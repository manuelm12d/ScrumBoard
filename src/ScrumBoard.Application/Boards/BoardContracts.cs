using ScrumBoard.Application.Tasks;

namespace ScrumBoard.Application.Boards;

public sealed record BoardColumnDto(Guid Id, Guid ProjectId, string Name, int Order);

public sealed record SaveColumnRequest(string Name);

public sealed record ReorderColumnsRequest(IReadOnlyList<Guid> OrderedColumnIds);

public sealed record BoardColumnWithTasksDto(Guid Id, string Name, int Order, IReadOnlyList<TaskDto> Tasks);

public sealed record BoardDto(Guid ProjectId, string ProjectName, IReadOnlyList<BoardColumnWithTasksDto> Columns);

/// <summary>Optional board filters, reused as-is by the report so both views always agree.</summary>
public sealed record BoardFilter(Guid? AssigneeId = null, int? Priority = null, string? Search = null)
{
    public static readonly BoardFilter None = new();
}
