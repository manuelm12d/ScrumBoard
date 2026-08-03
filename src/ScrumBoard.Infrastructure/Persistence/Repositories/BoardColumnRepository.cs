using Microsoft.EntityFrameworkCore;
using ScrumBoard.Application.Abstractions.Persistence;
using ScrumBoard.Domain.Boards;

namespace ScrumBoard.Infrastructure.Persistence.Repositories;

internal sealed class BoardColumnRepository : IBoardColumnRepository
{
    private readonly AppDbContext _context;

    public BoardColumnRepository(AppDbContext context) => _context = context;

    public Task<BoardColumn?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.BoardColumns.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyList<BoardColumn>> ListByProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
        => await _context.BoardColumns
            .Where(c => c.ProjectId == projectId)
            .OrderBy(c => c.Order)
            .ToListAsync(cancellationToken);

    public async Task<int?> GetMaxOrderAsync(Guid projectId, CancellationToken cancellationToken = default)
        => await _context.BoardColumns
            .Where(c => c.ProjectId == projectId)
            .MaxAsync(c => (int?)c.Order, cancellationToken);

    public Task<int> CountTasksAsync(Guid columnId, CancellationToken cancellationToken = default)
        => _context.Tasks.CountAsync(t => t.ColumnId == columnId, cancellationToken);

    public async Task AddAsync(BoardColumn column, CancellationToken cancellationToken = default)
        => await _context.BoardColumns.AddAsync(column, cancellationToken);

    public void Remove(BoardColumn column) => _context.BoardColumns.Remove(column);
}
