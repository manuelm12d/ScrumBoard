namespace ScrumBoard.Application.Abstractions.Realtime;

/// <summary>
/// Driven port for board broadcasting. The application layer describes what happened; the adapter
/// (SignalR today, anything else tomorrow) decides how it reaches the connected sessions.
/// </summary>
public interface IBoardNotifier
{
    Task PublishAsync(Guid projectId, BoardEvent boardEvent, CancellationToken cancellationToken = default);
}

public sealed record BoardEvent(string Type, object Payload);

public static class BoardEventTypes
{
    public const string TaskCreated = "task.created";
    public const string TaskUpdated = "task.updated";
    public const string TaskDeleted = "task.deleted";
    public const string TaskMoved = "task.moved";
    public const string ColumnsChanged = "columns.changed";
}
