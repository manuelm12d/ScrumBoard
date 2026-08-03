using ScrumBoard.Application.Boards;

namespace ScrumBoard.Application.Abstractions.Persistence;

/// <summary>
/// Read side port: returns the whole board in a single projected query instead of walking the
/// aggregate root and paying an N+1 per column.
/// </summary>
public interface IBoardQuery
{
    Task<BoardDto?> GetBoardAsync(Guid projectId, BoardFilter filter, CancellationToken cancellationToken = default);
}
