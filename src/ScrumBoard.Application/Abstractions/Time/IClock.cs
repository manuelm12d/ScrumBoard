namespace ScrumBoard.Application.Abstractions.Time;

/// <summary>Driven port for the system clock, so use cases stay deterministic under test.</summary>
public interface IClock
{
    DateTime UtcNow { get; }
}
