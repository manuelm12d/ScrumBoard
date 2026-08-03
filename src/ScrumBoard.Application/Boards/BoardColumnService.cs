using ScrumBoard.Application.Abstractions.Persistence;
using ScrumBoard.Application.Abstractions.Realtime;
using ScrumBoard.Application.Common;
using ScrumBoard.Domain.Boards;

namespace ScrumBoard.Application.Boards;

public sealed class BoardColumnService
{
    private readonly IBoardColumnRepository _columns;
    private readonly IProjectRepository _projects;
    private readonly IBoardQuery _boardQuery;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBoardNotifier _notifier;

    public BoardColumnService(
        IBoardColumnRepository columns,
        IProjectRepository projects,
        IBoardQuery boardQuery,
        IUnitOfWork unitOfWork,
        IBoardNotifier notifier)
    {
        _columns = columns;
        _projects = projects;
        _boardQuery = boardQuery;
        _unitOfWork = unitOfWork;
        _notifier = notifier;
    }

    public async Task<BoardDto> GetBoardAsync(Guid projectId, BoardFilter filter, CancellationToken cancellationToken = default)
        => await _boardQuery.GetBoardAsync(projectId, filter, cancellationToken)
           ?? throw new NotFoundException("el proyecto", projectId);

    public async Task<IReadOnlyList<BoardColumnDto>> ListAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        await EnsureProjectExistsAsync(projectId, cancellationToken);
        var columns = await _columns.ListByProjectAsync(projectId, cancellationToken);
        return columns.Select(Map).ToList();
    }

    public async Task<BoardColumnDto> CreateAsync(Guid projectId, SaveColumnRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureProjectExistsAsync(projectId, cancellationToken);

        var maxOrder = await _columns.GetMaxOrderAsync(projectId, cancellationToken);
        var column = new BoardColumn(Guid.NewGuid(), projectId, request.Name, (maxOrder ?? -1) + 1);

        await _columns.AddAsync(column, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await PublishColumnsChangedAsync(projectId, cancellationToken);

        return Map(column);
    }

    public async Task<BoardColumnDto> UpdateAsync(Guid columnId, SaveColumnRequest request, CancellationToken cancellationToken = default)
    {
        var column = await GetColumnAsync(columnId, cancellationToken);
        column.Rename(request.Name);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await PublishColumnsChangedAsync(column.ProjectId, cancellationToken);

        return Map(column);
    }

    public async Task DeleteAsync(Guid columnId, CancellationToken cancellationToken = default)
    {
        var column = await GetColumnAsync(columnId, cancellationToken);

        var taskCount = await _columns.CountTasksAsync(columnId, cancellationToken);
        column.EnsureCanBeDeleted(taskCount);

        _columns.Remove(column);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await PublishColumnsChangedAsync(column.ProjectId, cancellationToken);
    }

    /// <summary>
    /// Reordering receives the full ordered list of ids: the client already knows the intended layout
    /// and sending it whole keeps the operation idempotent and free of intermediate invalid states.
    /// </summary>
    public async Task<IReadOnlyList<BoardColumnDto>> ReorderAsync(
        Guid projectId,
        ReorderColumnsRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureProjectExistsAsync(projectId, cancellationToken);

        var columns = await _columns.ListByProjectAsync(projectId, cancellationToken);
        var requested = request.OrderedColumnIds;

        if (requested.Count != columns.Count || requested.Distinct().Count() != requested.Count
            || requested.Any(id => columns.All(c => c.Id != id)))
        {
            throw new ConflictException("La lista de reordenamiento debe contener exactamente las columnas del proyecto.");
        }

        for (var i = 0; i < requested.Count; i++)
        {
            columns.First(c => c.Id == requested[i]).MoveTo(i);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await PublishColumnsChangedAsync(projectId, cancellationToken);

        return columns.OrderBy(c => c.Order).Select(Map).ToList();
    }

    private async Task<BoardColumn> GetColumnAsync(Guid columnId, CancellationToken cancellationToken)
        => await _columns.GetByIdAsync(columnId, cancellationToken)
           ?? throw new NotFoundException("la columna", columnId);

    private async Task EnsureProjectExistsAsync(Guid projectId, CancellationToken cancellationToken)
    {
        if (!await _projects.ExistsAsync(projectId, cancellationToken))
        {
            throw new NotFoundException("el proyecto", projectId);
        }
    }

    private async Task PublishColumnsChangedAsync(Guid projectId, CancellationToken cancellationToken)
    {
        var columns = await _columns.ListByProjectAsync(projectId, cancellationToken);
        await _notifier.PublishAsync(
            projectId,
            new BoardEvent(BoardEventTypes.ColumnsChanged, columns.Select(Map).ToList()),
            cancellationToken);
    }

    private static BoardColumnDto Map(BoardColumn column) => new(column.Id, column.ProjectId, column.Name, column.Order);
}
