using ScrumBoard.Application.Abstractions.Persistence;
using ScrumBoard.Application.Abstractions.Realtime;
using ScrumBoard.Application.Abstractions.Time;
using ScrumBoard.Application.Common;
using ScrumBoard.Domain.Boards;
using ScrumBoard.Domain.Tasks;
using ScrumBoard.Domain.Users;

namespace ScrumBoard.Application.Tasks;

public sealed class TaskService
{
    private readonly ITaskRepository _tasks;
    private readonly IBoardColumnRepository _columns;
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBoardNotifier _notifier;
    private readonly IClock _clock;

    public TaskService(
        ITaskRepository tasks,
        IBoardColumnRepository columns,
        IUserRepository users,
        IUnitOfWork unitOfWork,
        IBoardNotifier notifier,
        IClock clock)
    {
        _tasks = tasks;
        _columns = columns;
        _users = users;
        _unitOfWork = unitOfWork;
        _notifier = notifier;
        _clock = clock;
    }

    public async Task<TaskDto> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var task = await GetTaskAsync(id, cancellationToken);
        var column = await GetColumnAsync(task.ColumnId, cancellationToken);
        return Map(task, column.ProjectId, task.Assignee?.Name);
    }

    public async Task<IReadOnlyList<TaskDto>> ListByColumnAsync(Guid columnId, CancellationToken cancellationToken = default)
    {
        var column = await GetColumnAsync(columnId, cancellationToken);
        var tasks = await _tasks.ListByColumnOrderedAsync(columnId, cancellationToken);
        return tasks.Select(t => Map(t, column.ProjectId, t.Assignee?.Name)).ToList();
    }

    public async Task<TaskDto> CreateAsync(CreateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var column = await GetColumnAsync(request.ColumnId, cancellationToken);
        var assignee = await ResolveAssigneeAsync(request.AssigneeId, cancellationToken);

        var lastPosition = await _tasks.GetMaxPositionAsync(column.Id, cancellationToken);
        var task = new WorkTask(
            Guid.NewGuid(),
            column.Id,
            request.Title,
            request.Description,
            request.Priority,
            request.AssigneeId,
            TaskPositionCalculator.Calculate(lastPosition, null),
            _clock.UtcNow);

        await _tasks.AddAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = Map(task, column.ProjectId, assignee?.Name);
        await _notifier.PublishAsync(column.ProjectId, new BoardEvent(BoardEventTypes.TaskCreated, dto), cancellationToken);
        return dto;
    }

    public async Task<TaskDto> UpdateAsync(Guid id, UpdateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var task = await GetTaskAsync(id, cancellationToken);
        var column = await GetColumnAsync(task.ColumnId, cancellationToken);
        var assignee = await ResolveAssigneeAsync(request.AssigneeId, cancellationToken);

        task.Update(request.Title, request.Description, request.Priority, request.AssigneeId);
        task.Touch(_clock.UtcNow);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = Map(task, column.ProjectId, assignee?.Name);
        await _notifier.PublishAsync(column.ProjectId, new BoardEvent(BoardEventTypes.TaskUpdated, dto), cancellationToken);
        return dto;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var task = await GetTaskAsync(id, cancellationToken);
        var column = await GetColumnAsync(task.ColumnId, cancellationToken);

        _tasks.Remove(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _notifier.PublishAsync(
            column.ProjectId,
            new BoardEvent(BoardEventTypes.TaskDeleted, new { TaskId = id, ColumnId = column.Id, ProjectId = column.ProjectId }),
            cancellationToken);
    }

    public async Task<TaskDto> MoveAsync(Guid id, MoveTaskRequest request, CancellationToken cancellationToken = default)
    {
        if (request.PreviousTaskId == id || request.NextTaskId == id)
        {
            throw new ConflictException("Una tarea no puede usarse como su propia referencia de posición.");
        }

        var task = await GetTaskAsync(id, cancellationToken);
        var originColumn = await GetColumnAsync(task.ColumnId, cancellationToken);
        var targetColumn = await GetColumnAsync(request.TargetColumnId, cancellationToken);

        if (originColumn.ProjectId != targetColumn.ProjectId)
        {
            throw new ConflictException("No se puede mover una tarea a un proyecto distinto.");
        }

        var previous = await ResolveNeighbourPositionAsync(request.PreviousTaskId, targetColumn.Id, cancellationToken);
        var next = await ResolveNeighbourPositionAsync(request.NextTaskId, targetColumn.Id, cancellationToken);

        decimal position;
        try
        {
            position = CalculatePosition(previous, next);
        }
        catch (PositionRebalanceRequiredException)
        {
            // The column ran out of representable gaps: renumber it and read the neighbours again.
            (previous, next) = await RebalanceColumnAsync(targetColumn.Id, request, cancellationToken);
            position = CalculatePosition(previous, next);
        }

        task.MoveTo(targetColumn.Id, position);
        task.Touch(_clock.UtcNow);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _notifier.PublishAsync(
            targetColumn.ProjectId,
            new BoardEvent(
                BoardEventTypes.TaskMoved,
                new TaskMovedEvent(task.Id, targetColumn.ProjectId, originColumn.Id, targetColumn.Id, position)),
            cancellationToken);

        return Map(task, targetColumn.ProjectId, task.Assignee?.Name);
    }

    private static decimal CalculatePosition(decimal? previous, decimal? next)
    {
        try
        {
            return TaskPositionCalculator.Calculate(previous, next);
        }
        catch (ArgumentException)
        {
            throw new ConflictException("Las tareas de referencia no reflejan el orden actual de la columna; recargue el tablero.");
        }
    }

    private async Task<(decimal? Previous, decimal? Next)> RebalanceColumnAsync(
        Guid columnId,
        MoveTaskRequest request,
        CancellationToken cancellationToken)
    {
        var tasks = await _tasks.ListByColumnOrderedAsync(columnId, cancellationToken);
        var positions = TaskPositionCalculator.Rebalance(tasks.Count);

        for (var i = 0; i < tasks.Count; i++)
        {
            tasks[i].MoveTo(columnId, positions[i]);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        decimal? Find(Guid? taskId) => taskId is null ? null : tasks.FirstOrDefault(t => t.Id == taskId)?.Position;
        return (Find(request.PreviousTaskId), Find(request.NextTaskId));
    }

    private async Task<decimal?> ResolveNeighbourPositionAsync(Guid? taskId, Guid columnId, CancellationToken cancellationToken)
    {
        if (taskId is null)
        {
            return null;
        }

        return await _tasks.GetPositionAsync(taskId.Value, columnId, cancellationToken)
               ?? throw new ConflictException($"La tarea de referencia {taskId} ya no pertenece a la columna destino.");
    }

    private async Task<User?> ResolveAssigneeAsync(Guid? assigneeId, CancellationToken cancellationToken)
    {
        if (assigneeId is null)
        {
            return null;
        }

        return await _users.GetByIdAsync(assigneeId.Value, cancellationToken)
               ?? throw new NotFoundException("el usuario responsable", assigneeId.Value);
    }

    private async Task<WorkTask> GetTaskAsync(Guid id, CancellationToken cancellationToken)
        => await _tasks.GetByIdAsync(id, cancellationToken) ?? throw new NotFoundException("la tarea", id);

    private async Task<BoardColumn> GetColumnAsync(Guid id, CancellationToken cancellationToken)
        => await _columns.GetByIdAsync(id, cancellationToken) ?? throw new NotFoundException("la columna", id);

    private static TaskDto Map(WorkTask task, Guid projectId, string? assigneeName) => new(
        task.Id,
        task.ColumnId,
        projectId,
        task.Title,
        task.Description,
        task.Priority,
        task.AssigneeId,
        assigneeName,
        task.Position,
        task.CreatedAtUtc);
}
