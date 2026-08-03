using ScrumBoard.Domain.Tasks;

namespace ScrumBoard.UnitTests.Domain;

/// <summary>
/// Mandatory coverage (requirement 6.9): the new position of a task when it is reordered.
/// </summary>
public class TaskPositionCalculatorTests
{
    [Fact]
    public void Calculate_BetweenTwoTasks_ReturnsMidpoint()
    {
        var position = TaskPositionCalculator.Calculate(1024m, 2048m);

        Assert.Equal(1536m, position);
    }

    [Fact]
    public void Calculate_OnEmptyColumn_ReturnsFirstStep()
    {
        var position = TaskPositionCalculator.Calculate(null, null);

        Assert.Equal(TaskPositionCalculator.Step, position);
    }

    [Fact]
    public void Calculate_AtTheTopOfTheColumn_ReturnsHalfOfTheFirstPosition()
    {
        var position = TaskPositionCalculator.Calculate(null, 1024m);

        Assert.Equal(512m, position);
    }

    [Fact]
    public void Calculate_AtTheBottomOfTheColumn_AppendsOneStep()
    {
        var position = TaskPositionCalculator.Calculate(3072m, null);

        Assert.Equal(3072m + TaskPositionCalculator.Step, position);
    }

    [Fact]
    public void Calculate_KeepsResultStrictlyBetweenNeighbours()
    {
        const decimal previous = 10.5m;
        const decimal next = 10.75m;

        var position = TaskPositionCalculator.Calculate(previous, next);

        Assert.True(position > previous);
        Assert.True(position < next);
    }

    [Fact]
    public void Calculate_WhenNeighboursAreExhausted_RequestsRebalance()
    {
        var exception = Assert.Throws<PositionRebalanceRequiredException>(
            () => TaskPositionCalculator.Calculate(1m, 1.0000001m));

        Assert.Equal(1m, exception.Previous);
    }

    [Fact]
    public void Calculate_WithNeighboursInTheWrongOrder_IsRejected()
    {
        Assert.Throws<ArgumentException>(() => TaskPositionCalculator.Calculate(2048m, 1024m));
    }

    [Fact]
    public void Rebalance_SpreadsPositionsEvenlyAndKeepsThemAscending()
    {
        var positions = TaskPositionCalculator.Rebalance(4);

        Assert.Equal(new[] { 1024m, 2048m, 3072m, 4096m }, positions);
    }
}
