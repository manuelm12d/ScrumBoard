using ScrumBoard.Domain.Common;

namespace ScrumBoard.Domain.Tasks;

/// <summary>
/// Raised when two neighbours are so close that no representable midpoint is left between them.
/// It is not a user error: the application layer answers by renumbering the column and retrying.
/// </summary>
public sealed class PositionRebalanceRequiredException : DomainException
{
    public PositionRebalanceRequiredException(decimal? previous, decimal? next)
        : base($"No queda espacio entre las posiciones {previous?.ToString() ?? "inicio"} y {next?.ToString() ?? "fin"}; se requiere rebalanceo.")
    {
        Previous = previous;
        Next = next;
    }

    public decimal? Previous { get; }

    public decimal? Next { get; }
}
