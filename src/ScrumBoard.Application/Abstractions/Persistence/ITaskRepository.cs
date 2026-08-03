using ScrumBoard.Domain.Tasks;

namespace ScrumBoard.Application.Abstractions.Persistence;

public interface ITaskRepository
{
    Task<WorkTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WorkTask>> ListByColumnOrderedAsync(Guid columnId, CancellationToken cancellationToken = default);

    /// <summary>Last position of a column, used to append a new task without loading the column.</summary>
    Task<decimal?> GetMaxPositionAsync(Guid columnId, CancellationToken cancellationToken = default);

    Task<decimal?> GetPositionAsync(Guid taskId, Guid columnId, CancellationToken cancellationToken = default);

    Task AddAsync(WorkTask task, CancellationToken cancellationToken = default);

    void Remove(WorkTask task);
}
