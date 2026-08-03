using ScrumBoard.Domain.Projects;
using ScrumBoard.Domain.Tasks;

namespace ScrumBoard.Application.Reports;

/// <summary>
/// The single transfer structure that feeds every output format (requirement 6.8). Exporters only
/// know this shape, never the persistence model, so a new format cannot drag EF into the picture.
/// </summary>
public sealed record ProjectReportDto(
    Guid ProjectId,
    string ProjectName,
    string? Description,
    DateOnly StartDate,
    DateOnly? PlannedEndDate,
    ProjectStatus Status,
    DateTime GeneratedAtUtc,
    IReadOnlyList<ProjectReportRow> Rows);

public sealed record ProjectReportRow(
    string TaskTitle,
    string ColumnName,
    string AssigneeName,
    TaskPriority Priority,
    DateTime CreatedAtUtc);

public sealed record ExportedFile(string FileName, string ContentType, byte[] Content);

public sealed class UnsupportedReportFormatException : Exception
{
    public UnsupportedReportFormatException(string format, IEnumerable<string> supported)
        : base($"El formato '{format}' no está soportado. Formatos disponibles: {string.Join(", ", supported)}.")
    {
    }
}
