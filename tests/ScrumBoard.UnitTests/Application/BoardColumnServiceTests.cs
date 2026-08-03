using ScrumBoard.Application.Abstractions.Persistence;
using ScrumBoard.Application.Boards;
using ScrumBoard.Domain.Boards;
using ScrumBoard.Domain.Common;
using ScrumBoard.Domain.Projects;
using ScrumBoard.UnitTests.Fakes;

namespace ScrumBoard.UnitTests.Application;

public class BoardColumnServiceTests
{
    private readonly FakeBoardColumnRepository _columns = new();
    private readonly FakeProjectRepository _projects = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeBoardNotifier _notifier = new();
    private readonly Project _project = new(
        Guid.NewGuid(),
        "Plataforma Scrum",
        null,
        new DateOnly(2026, 1, 1),
        null,
        ProjectStatus.Active,
        DateTime.UtcNow);

    private BoardColumnService CreateService()
    {
        _projects.Items.Add(_project);
        return new BoardColumnService(_columns, _projects, new NullBoardQuery(), _unitOfWork, _notifier);
    }

    [Fact]
    public async Task DeleteAsync_WhenTheColumnHasTasks_IsBlockedByTheBusinessRule()
    {
        var service = CreateService();
        var column = new BoardColumn(Guid.NewGuid(), _project.Id, "En progreso", 0);
        _columns.Items.Add(column);
        _columns.TaskCounter = _ => 2;

        await Assert.ThrowsAsync<ColumnNotEmptyException>(() => service.DeleteAsync(column.Id));
        Assert.Contains(column, _columns.Items);
    }

    [Fact]
    public async Task ReorderAsync_RewritesTheOrderFollowingTheRequestedSequence()
    {
        var service = CreateService();
        var backlog = new BoardColumn(Guid.NewGuid(), _project.Id, "Backlog", 0);
        var doing = new BoardColumn(Guid.NewGuid(), _project.Id, "En progreso", 1);
        var done = new BoardColumn(Guid.NewGuid(), _project.Id, "Hecho", 2);
        _columns.Items.AddRange(new[] { backlog, doing, done });

        var result = await service.ReorderAsync(
            _project.Id,
            new ReorderColumnsRequest(new[] { done.Id, backlog.Id, doing.Id }));

        Assert.Equal(new[] { "Hecho", "Backlog", "En progreso" }, result.Select(c => c.Name));
        Assert.Equal(0, done.Order);
        Assert.Equal(2, doing.Order);
    }

    [Fact]
    public async Task CreateAsync_AppendsTheColumnAtTheEnd()
    {
        var service = CreateService();
        _columns.Items.Add(new BoardColumn(Guid.NewGuid(), _project.Id, "Backlog", 0));

        var created = await service.CreateAsync(_project.Id, new SaveColumnRequest("Hecho"));

        Assert.Equal(1, created.Order);
    }

    private sealed class NullBoardQuery : IBoardQuery
    {
        public Task<BoardDto?> GetBoardAsync(Guid projectId, BoardFilter filter, CancellationToken cancellationToken = default)
            => Task.FromResult<BoardDto?>(null);
    }
}
