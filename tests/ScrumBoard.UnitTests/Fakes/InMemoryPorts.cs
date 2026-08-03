using ScrumBoard.Application.Abstractions.Persistence;
using ScrumBoard.Application.Abstractions.Realtime;
using ScrumBoard.Application.Abstractions.Time;
using ScrumBoard.Application.Common;
using ScrumBoard.Domain.Boards;
using ScrumBoard.Domain.Projects;
using ScrumBoard.Domain.Tasks;
using ScrumBoard.Domain.Users;

namespace ScrumBoard.UnitTests.Fakes;

/// <summary>
/// Hand written adapters for the driven ports. Because the application layer only depends on the
/// interfaces, the use cases can be exercised without a database or a mocking framework.
/// </summary>
internal sealed class FakeTaskRepository : ITaskRepository
{
    public List<WorkTask> Items { get; } = new();

    public Task<WorkTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(Items.FirstOrDefault(t => t.Id == id));

    public Task<IReadOnlyList<WorkTask>> ListByColumnOrderedAsync(Guid columnId, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<WorkTask>>(
            Items.Where(t => t.ColumnId == columnId).OrderBy(t => t.Position).ToList());

    public Task<decimal?> GetMaxPositionAsync(Guid columnId, CancellationToken cancellationToken = default)
        => Task.FromResult(Items.Where(t => t.ColumnId == columnId)
            .Select(t => (decimal?)t.Position)
            .DefaultIfEmpty(null)
            .Max());

    public Task<decimal?> GetPositionAsync(Guid taskId, Guid columnId, CancellationToken cancellationToken = default)
        => Task.FromResult(Items
            .Where(t => t.Id == taskId && t.ColumnId == columnId)
            .Select(t => (decimal?)t.Position)
            .FirstOrDefault());

    public Task AddAsync(WorkTask task, CancellationToken cancellationToken = default)
    {
        Items.Add(task);
        return Task.CompletedTask;
    }

    public void Remove(WorkTask task) => Items.Remove(task);
}

internal sealed class FakeBoardColumnRepository : IBoardColumnRepository
{
    public List<BoardColumn> Items { get; } = new();

    public Func<Guid, int> TaskCounter { get; set; } = _ => 0;

    public Task<BoardColumn?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(Items.FirstOrDefault(c => c.Id == id));

    public Task<IReadOnlyList<BoardColumn>> ListByProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<BoardColumn>>(
            Items.Where(c => c.ProjectId == projectId).OrderBy(c => c.Order).ToList());

    public Task<int?> GetMaxOrderAsync(Guid projectId, CancellationToken cancellationToken = default)
        => Task.FromResult(Items.Where(c => c.ProjectId == projectId)
            .Select(c => (int?)c.Order)
            .DefaultIfEmpty(null)
            .Max());

    public Task<int> CountTasksAsync(Guid columnId, CancellationToken cancellationToken = default)
        => Task.FromResult(TaskCounter(columnId));

    public Task AddAsync(BoardColumn column, CancellationToken cancellationToken = default)
    {
        Items.Add(column);
        return Task.CompletedTask;
    }

    public void Remove(BoardColumn column) => Items.Remove(column);
}

internal sealed class FakeUserRepository : IUserRepository
{
    public List<User> Items { get; } = new();

    public Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
        => Task.FromResult(Items.FirstOrDefault(u => u.Email == normalizedEmail));

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(Items.FirstOrDefault(u => u.Id == id));

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(Items.Any(u => u.Id == id));

    public Task<IReadOnlyList<User>> ListAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<User>>(Items);
}

internal sealed class FakeProjectRepository : IProjectRepository
{
    public List<Project> Items { get; } = new();

    public Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(Items.FirstOrDefault(p => p.Id == id));

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(Items.Any(p => p.Id == id));

    public Task<PagedResult<Project>> SearchAsync(string? nameFilter, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var filtered = Items
            .Where(p => nameFilter is null || p.Name.Contains(nameFilter, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var items = filtered.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Task.FromResult(new PagedResult<Project>(items, page, pageSize, filtered.Count));
    }

    public Task AddAsync(Project project, CancellationToken cancellationToken = default)
    {
        Items.Add(project);
        return Task.CompletedTask;
    }

    public void Remove(Project project) => Items.Remove(project);
}

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveCount { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveCount++;
        return Task.FromResult(0);
    }
}

internal sealed class FakeBoardNotifier : IBoardNotifier
{
    public List<(Guid ProjectId, BoardEvent Event)> Published { get; } = new();

    public Task PublishAsync(Guid projectId, BoardEvent boardEvent, CancellationToken cancellationToken = default)
    {
        Published.Add((projectId, boardEvent));
        return Task.CompletedTask;
    }
}

internal sealed class FixedClock : IClock
{
    public FixedClock(DateTime utcNow) => UtcNow = utcNow;

    public DateTime UtcNow { get; }
}
