using Microsoft.EntityFrameworkCore;
using ScrumBoard.Application.Abstractions.Persistence;
using ScrumBoard.Domain.Tasks;

namespace ScrumBoard.Infrastructure.Persistence.Repositories;

internal sealed class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _context;

    public TaskRepository(AppDbContext context) => _context = context;

    public Task<WorkTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.Tasks.Include(t => t.Assignee).FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<IReadOnlyList<WorkTask>> ListByColumnOrderedAsync(Guid columnId, CancellationToken cancellationToken = default)
        => await _context.Tasks
            .Include(t => t.Assignee)
            .Where(t => t.ColumnId == columnId)
            .OrderBy(t => t.Position)
            .ToListAsync(cancellationToken);

    public async Task<decimal?> GetMaxPositionAsync(Guid columnId, CancellationToken cancellationToken = default)
        => await _context.Tasks
            .Where(t => t.ColumnId == columnId)
            .MaxAsync(t => (decimal?)t.Position, cancellationToken);

    public async Task<decimal?> GetPositionAsync(Guid taskId, Guid columnId, CancellationToken cancellationToken = default)
        => await _context.Tasks
            .Where(t => t.Id == taskId && t.ColumnId == columnId)
            .Select(t => (decimal?)t.Position)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task AddAsync(WorkTask task, CancellationToken cancellationToken = default)
        => await _context.Tasks.AddAsync(task, cancellationToken);

    public void Remove(WorkTask task) => _context.Tasks.Remove(task);
}
