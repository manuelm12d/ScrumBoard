namespace ScrumBoard.Domain.Tasks;

/// <summary>
/// Fractional indexing for the kanban board. Dropping a task between two neighbours only needs the
/// midpoint of their positions, so a move is a single-row UPDATE instead of renumbering the column.
/// </summary>
public static class TaskPositionCalculator
{
    /// <summary>Distance between consecutive tasks when a column is created or rebalanced.</summary>
    public const decimal Step = 1024m;

    /// <summary>
    /// Smallest gap still worth splitting. The column is stored as numeric(18,9); stopping at 1e-6
    /// keeps three spare decimal places so the midpoint never rounds onto one of its neighbours.
    /// </summary>
    public const decimal MinimumGap = 0.000001m;

    /// <summary>
    /// Position for a task dropped between <paramref name="previous"/> and <paramref name="next"/>.
    /// Both are null for an empty column, <paramref name="previous"/> is null at the top of the list
    /// and <paramref name="next"/> is null at the bottom.
    /// </summary>
    /// <exception cref="PositionRebalanceRequiredException">
    /// The neighbours are closer than <see cref="MinimumGap"/> and the column must be renumbered first.
    /// </exception>
    public static decimal Calculate(decimal? previous, decimal? next)
    {
        if (previous is not null && next is not null && previous >= next)
        {
            throw new ArgumentException(
                $"La posición anterior ({previous}) debe ser menor que la siguiente ({next}).",
                nameof(previous));
        }

        return (previous, next) switch
        {
            (null, null) => Step,
            (null, { } first) => SplitFromZero(first),
            ({ } last, null) => last + Step,
            ({ } before, { } after) => SplitBetween(before, after)
        };
    }

    /// <summary>Positions of a whole column after a rebalance, evenly spaced by <see cref="Step"/>.</summary>
    public static IReadOnlyList<decimal> Rebalance(int taskCount)
    {
        if (taskCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(taskCount));
        }

        var positions = new decimal[taskCount];
        for (var i = 0; i < taskCount; i++)
        {
            positions[i] = Step * (i + 1);
        }

        return positions;
    }

    private static decimal SplitFromZero(decimal next)
    {
        if (next < MinimumGap)
        {
            throw new PositionRebalanceRequiredException(null, next);
        }

        return next / 2m;
    }

    private static decimal SplitBetween(decimal previous, decimal next)
    {
        if (next - previous < MinimumGap)
        {
            throw new PositionRebalanceRequiredException(previous, next);
        }

        return previous + ((next - previous) / 2m);
    }
}
