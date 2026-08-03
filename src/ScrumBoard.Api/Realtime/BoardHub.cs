using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ScrumBoard.Application.Abstractions.Persistence;

namespace ScrumBoard.Api.Realtime;

/// <summary>
/// Driving adapter for real time. One SignalR group per project is what guarantees requirement 6.7:
/// a session only receives events of the boards it explicitly joined.
/// </summary>
[Authorize]
public sealed class BoardHub : Hub
{
    private readonly IProjectRepository _projects;
    private readonly ILogger<BoardHub> _logger;

    public BoardHub(IProjectRepository projects, ILogger<BoardHub> logger)
    {
        _projects = projects;
        _logger = logger;
    }

    public static string GroupName(Guid projectId) => $"board:{projectId}";

    public async Task JoinBoard(Guid projectId)
    {
        if (!await _projects.ExistsAsync(projectId, Context.ConnectionAborted))
        {
            throw new HubException($"El proyecto {projectId} no existe.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(projectId), Context.ConnectionAborted);
        _logger.LogInformation("Conexión {ConnectionId} suscrita al tablero {ProjectId}", Context.ConnectionId, projectId);
    }

    public async Task LeaveBoard(Guid projectId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(projectId), Context.ConnectionAborted);
        _logger.LogInformation("Conexión {ConnectionId} abandonó el tablero {ProjectId}", Context.ConnectionId, projectId);
    }

    // Group membership is dropped automatically with the connection, so no manual cleanup is needed
    // here; the override only records the disconnection reason.
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception is not null)
        {
            _logger.LogWarning(exception, "Conexión {ConnectionId} finalizada con error", Context.ConnectionId);
        }

        return base.OnDisconnectedAsync(exception);
    }
}
