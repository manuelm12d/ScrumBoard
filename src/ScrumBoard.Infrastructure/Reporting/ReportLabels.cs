using ScrumBoard.Domain.Projects;
using ScrumBoard.Domain.Tasks;

namespace ScrumBoard.Infrastructure.Reporting;

internal static class ReportLabels
{
    internal static string Priority(TaskPriority priority) => priority switch
    {
        TaskPriority.Low => "Baja",
        TaskPriority.Medium => "Media",
        TaskPriority.High => "Alta",
        TaskPriority.Critical => "Crítica",
        _ => priority.ToString()
    };

    internal static string Status(ProjectStatus status) => status switch
    {
        ProjectStatus.Planned => "Planificado",
        ProjectStatus.Active => "Activo",
        ProjectStatus.OnHold => "En pausa",
        ProjectStatus.Completed => "Completado",
        ProjectStatus.Cancelled => "Cancelado",
        _ => status.ToString()
    };

    internal static string Date(DateOnly? date) => date?.ToString("dd/MM/yyyy") ?? "-";

    internal static string DateTimeUtc(DateTime value) => value.ToString("dd/MM/yyyy HH:mm 'UTC'");
}
