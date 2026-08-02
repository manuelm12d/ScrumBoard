using ScrumBoard.Application.Abstractions.Time;

namespace ScrumBoard.Infrastructure.Time;

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
