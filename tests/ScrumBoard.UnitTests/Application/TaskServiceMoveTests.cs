using ScrumBoard.Application.Abstractions.Realtime;
using ScrumBoard.Application.Common;
using ScrumBoard.Application.Tasks;
using ScrumBoard.Domain.Boards;
using ScrumBoard.Domain.Tasks;
using ScrumBoard.UnitTests.Fakes;

namespace ScrumBoard.UnitTests.Application;

public class TaskServiceMoveTests
{
    private static readonly Guid ProjectId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 7, 31, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakeTaskRepository _tasks = new();
    private readonly FakeBoardColumnRepository _columns = new();
    private readonly FakeUserRepository _users = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeBoardNotifier _notifier = new();

    private readonly BoardColumn _todo = new(Guid.NewGuid(), ProjectId, "Por hacer", 0);
    private readonly BoardColumn _doing = new(Guid.NewGuid(), ProjectId, "En progreso", 1);

    private TaskService CreateService()
    {
        _columns.Items.AddRange(new[] { _todo, _doing });
        return new TaskService(_tasks, _columns, _users, _unitOfWork, _notifier, new FixedClock(Now));
    }

    private WorkTask AddTask(BoardColumn column, string title, decimal position)
    {
        var task = new WorkTask(Guid.NewGuid(), column.Id, title, null, TaskPriority.Medium, null, position, Now);
        _tasks.Items.Add(task);
        return task;
    }

    [Fact]
    public async Task MoveAsync_BetweenTwoTasksOfAnotherColumn_LandsOnTheMidpoint()
    {
        var service = CreateService();
        var first = AddTask(_doing, "Primera", 1024m);
        var second = AddTask(_doing, "Segunda", 2048m);
        var moving = AddTask(_todo, "Movida", 1024m);

        var result = await service.MoveAsync(moving.Id, new MoveTaskRequest(_doing.Id, first.Id, second.Id));

        Assert.Equal(1536m, result.Position);
        Assert.Equal(_doing.Id, result.ColumnId);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task MoveAsync_PublishesTheEventToTheProjectBoard()
    {
        var service = CreateService();
        var moving = AddTask(_todo, "Movida", 1024m);

        await service.MoveAsync(moving.Id, new MoveTaskRequest(_doing.Id, null, null));

        var (projectId, published) = Assert.Single(_notifier.Published);
        Assert.Equal(ProjectId, projectId);
        Assert.Equal(BoardEventTypes.TaskMoved, published.Type);

        var payload = Assert.IsType<TaskMovedEvent>(published.Payload);
        Assert.Equal(_todo.Id, payload.FromColumnId);
        Assert.Equal(_doing.Id, payload.ToColumnId);
    }

    [Fact]
    public async Task MoveAsync_WhenNeighboursRunOutOfGap_RebalancesTheColumnAndKeepsTheOrder()
    {
        var service = CreateService();
        var first = AddTask(_doing, "Primera", 1m);
        var second = AddTask(_doing, "Segunda", 1.0000001m);
        var moving = AddTask(_todo, "Movida", 5m);

        var result = await service.MoveAsync(moving.Id, new MoveTaskRequest(_doing.Id, first.Id, second.Id));

        Assert.Equal(TaskPositionCalculator.Step, first.Position);
        Assert.Equal(TaskPositionCalculator.Step * 2, second.Position);
        Assert.True(result.Position > first.Position && result.Position < second.Position);
    }

    [Fact]
    public async Task MoveAsync_WithANeighbourOutsideTheTargetColumn_IsRejected()
    {
        var service = CreateService();
        var strayNeighbour = AddTask(_todo, "Vecina de otra columna", 1024m);
        var moving = AddTask(_todo, "Movida", 2048m);

        await Assert.ThrowsAsync<ConflictException>(
            () => service.MoveAsync(moving.Id, new MoveTaskRequest(_doing.Id, strayNeighbour.Id, null)));
    }

    [Fact]
    public async Task CreateAsync_AppendsTheTaskAtTheEndOfTheColumn()
    {
        var service = CreateService();
        AddTask(_todo, "Existente", 1024m);

        var created = await service.CreateAsync(
            new CreateTaskRequest(_todo.Id, "Nueva", null, TaskPriority.High, null));

        Assert.Equal(1024m + TaskPositionCalculator.Step, created.Position);
        Assert.Equal(BoardEventTypes.TaskCreated, Assert.Single(_notifier.Published).Event.Type);
    }
}
