using ScrumBoard.Application.Reports;

namespace ScrumBoard.Application.Abstractions.Reporting;

/// <summary>
/// Strategy for turning the report DTO into a downloadable file. Adding a third format means adding a
/// new implementation and registering it; existing exporters stay untouched (requirement 6.8).
/// </summary>
public interface IReportExporter
{
    /// <summary>Key used by the client, e.g. "pdf" or "xlsx".</summary>
    string Format { get; }

    string ContentType { get; }

    string FileExtension { get; }

    byte[] Export(ProjectReportDto report);
}
