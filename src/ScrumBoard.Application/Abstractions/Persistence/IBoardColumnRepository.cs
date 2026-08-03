using ScrumBoard.Domain.Boards;

namespace ScrumBoard.Application.Abstractions.Persistence;

public interface IBoardColumnRepository
{
    Task<BoardColumn?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BoardColumn>> ListByProjectAsync(Guid projectId, CancellationToken cancellationToken = default);

    Task<int?> GetMaxOrderAsync(Guid projectId, CancellationToken cancellationToken = default);

    Task<int> CountTasksAsync(Guid columnId, CancellationToken cancellationToken = default);

    Task AddAsync(BoardColumn column, CancellationToken cancellationToken = default);

    void Remove(BoardColumn column);
}
