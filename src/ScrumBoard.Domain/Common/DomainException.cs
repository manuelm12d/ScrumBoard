namespace ScrumBoard.Domain.Common;

/// <summary>
/// Base of every rule violation expressed by the domain. The API maps it to a 4xx response,
/// so throwing it is the way the domain rejects an operation without knowing about HTTP.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message)
    {
    }
}

public sealed class DomainValidationException : DomainException
{
    public DomainValidationException(string message) : base(message)
    {
    }
}

public sealed class ColumnNotEmptyException : DomainException
{
    public ColumnNotEmptyException(Guid columnId, int taskCount)
        : base($"La columna {columnId} no puede eliminarse porque contiene {taskCount} tarea(s).")
    {
        ColumnId = columnId;
        TaskCount = taskCount;
    }

    public Guid ColumnId { get; }

    public int TaskCount { get; }
}
