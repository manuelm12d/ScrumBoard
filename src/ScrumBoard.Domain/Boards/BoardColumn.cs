using ScrumBoard.Domain.Common;
using ScrumBoard.Domain.Projects;
using ScrumBoard.Domain.Tasks;

namespace ScrumBoard.Domain.Boards;

/// <summary>
/// Workflow column of a project. Its ordering uses contiguous integers because columns are few
/// and are reordered as a whole list, unlike tasks which use fractional positions.
/// </summary>
public class BoardColumn
{
    private readonly List<WorkTask> _tasks = new();

    private BoardColumn()
    {
    }

    public BoardColumn(Guid id, Guid projectId, string name, int order)
    {
        Id = id;
        ProjectId = projectId;
        Rename(name);
        Order = order;
    }

    public Guid Id { get; private set; }

    public Guid ProjectId { get; private set; }

    public Project? Project { get; private set; }

    public string Name { get; private set; } = null!;

    public int Order { get; private set; }

    public IReadOnlyCollection<WorkTask> Tasks => _tasks;

    public void Rename(string name) => Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name), 100);

    public void MoveTo(int order)
    {
        if (order < 0)
        {
            throw new DomainValidationException("El orden de una columna no puede ser negativo.");
        }

        Order = order;
    }

    /// <summary>
    /// Business rule 6.4: a column holding tasks cannot be removed. The caller supplies the count so the
    /// rule stays enforceable without forcing the whole task collection to be loaded into memory.
    /// </summary>
    public void EnsureCanBeDeleted(int taskCount)
    {
        if (taskCount > 0)
        {
            throw new ColumnNotEmptyException(Id, taskCount);
        }
    }
}
