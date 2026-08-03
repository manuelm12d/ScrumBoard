using ScrumBoard.Application.Abstractions.Reporting;
using ScrumBoard.Application.Boards;
using ScrumBoard.Application.Common;

namespace ScrumBoard.Application.Reports;

public sealed class ReportService
{
    private readonly IProjectReportQuery _query;
    private readonly IReadOnlyDictionary<string, IReportExporter> _exporters;

    public ReportService(IProjectReportQuery query, IEnumerable<IReportExporter> exporters)
    {
        _query = query;
        _exporters = exporters.ToDictionary(e => e.Format, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyCollection<string> SupportedFormats => _exporters.Keys.ToList();

    public async Task<ExportedFile> GenerateAsync(
        Guid projectId,
        string format,
        BoardFilter filter,
        CancellationToken cancellationToken = default)
    {
        if (!_exporters.TryGetValue(format, out var exporter))
        {
            throw new UnsupportedReportFormatException(format, _exporters.Keys);
        }

        var report = await _query.GetAsync(projectId, filter, cancellationToken)
                     ?? throw new NotFoundException("el proyecto", projectId);

        var content = exporter.Export(report);
        var fileName = $"{Slugify(report.ProjectName)}-{report.GeneratedAtUtc:yyyyMMdd-HHmm}.{exporter.FileExtension}";
        return new ExportedFile(fileName, exporter.ContentType, content);
    }

    private static string Slugify(string value)
    {
        var chars = value.Trim().ToLowerInvariant()
            .Select(c => char.IsLetterOrDigit(c) ? c : '-')
            .ToArray();

        var slug = new string(chars);
        while (slug.Contains("--", StringComparison.Ordinal))
        {
            slug = slug.Replace("--", "-", StringComparison.Ordinal);
        }

        return slug.Trim('-') is { Length: > 0 } cleaned ? cleaned : "reporte";
    }
}
