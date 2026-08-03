using ScrumBoard.Domain.Boards;
using ScrumBoard.Domain.Common;

namespace ScrumBoard.UnitTests.Domain;

public class BoardColumnTests
{
    [Fact]
    public void EnsureCanBeDeleted_WithTasks_ThrowsBusinessRule()
    {
        var column = new BoardColumn(Guid.NewGuid(), Guid.NewGuid(), "En progreso", 0);

        var exception = Assert.Throws<ColumnNotEmptyException>(() => column.EnsureCanBeDeleted(3));

        Assert.Equal(3, exception.TaskCount);
    }

    [Fact]
    public void EnsureCanBeDeleted_WithoutTasks_DoesNotThrow()
    {
        var column = new BoardColumn(Guid.NewGuid(), Guid.NewGuid(), "Hecho", 2);

        column.EnsureCanBeDeleted(0);
    }

    [Fact]
    public void Rename_WithBlankName_IsRejected()
    {
        var column = new BoardColumn(Guid.NewGuid(), Guid.NewGuid(), "Backlog", 0);

        Assert.Throws<DomainValidationException>(() => column.Rename("   "));
    }
}
