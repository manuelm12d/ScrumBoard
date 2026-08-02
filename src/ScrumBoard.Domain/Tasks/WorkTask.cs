using ScrumBoard.Domain.Boards;
using ScrumBoard.Domain.Common;
using ScrumBoard.Domain.Users;

namespace ScrumBoard.Domain.Tasks;

/// <summary>
/// Named WorkTask instead of Task to avoid colliding with System.Threading.Tasks.Task in every file
/// of the solution that also does async work.
/// </summary>
public class WorkTask
{
    private WorkTask()
    {
    }

    public WorkTask(
        Guid id,
        Guid columnId,
        string title,
        string? description,
        TaskPriority priority,
        Guid? assigneeId,
        decimal position,
        DateTime createdAtUtc)
    {
        Id = id;
        ColumnId = columnId;
        Position = position;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
        Update(title, description, priority, assigneeId);
    }

    public Guid Id { get; private set; }

    public Guid ColumnId { get; private set; }

    public BoardColumn? Column { get; private set; }

    public string Title { get; private set; } = null!;

    public string? Description { get; private set; }

    public TaskPriority Priority { get; private set; }

    public Guid? AssigneeId { get; private set; }

    public User? Assignee { get; private set; }

    /// <summary>
    /// Fractional index inside its column. Moving a task rewrites this single row instead of
    /// renumbering the whole column. See TaskPositionCalculator.
    /// </summary>
    public decimal Position { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public void Update(string title, string? description, TaskPriority priority, Guid? assigneeId)
    {
        Title = Guard.AgainstNullOrWhiteSpace(title, nameof(title), 200);
        Description = Guard.AgainstTooLong(description, nameof(description), 4000);
        Priority = priority;
        AssigneeId = assigneeId;
    }

    public void MoveTo(Guid columnId, decimal position)
    {
        if (position <= 0)
        {
            throw new DomainValidationException("La posición de una tarea debe ser mayor que cero.");
        }

        ColumnId = columnId;
        Position = position;
    }

    public void Touch(DateTime utcNow) => UpdatedAtUtc = utcNow;
}
