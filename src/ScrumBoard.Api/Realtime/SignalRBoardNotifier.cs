using Microsoft.AspNetCore.SignalR;
using ScrumBoard.Application.Abstractions.Realtime;

namespace ScrumBoard.Api.Realtime;

/// <summary>Driven adapter that turns application board events into SignalR group messages.</summary>
internal sealed class SignalRBoardNotifier : IBoardNotifier
{
    private readonly IHubContext<BoardHub> _hub;

    public SignalRBoardNotifier(IHubContext<BoardHub> hub) => _hub = hub;

    public Task PublishAsync(Guid projectId, BoardEvent boardEvent, CancellationToken cancellationToken = default)
        => _hub.Clients
            .Group(BoardHub.GroupName(projectId))
            .SendAsync("boardEvent", boardEvent, cancellationToken);
}
