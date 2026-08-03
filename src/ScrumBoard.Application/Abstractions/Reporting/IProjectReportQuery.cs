using ScrumBoard.Application.Boards;
using ScrumBoard.Application.Reports;

namespace ScrumBoard.Application.Abstractions.Reporting;

/// <summary>Single database round trip that feeds every export format.</summary>
public interface IProjectReportQuery
{
    Task<ProjectReportDto?> GetAsync(Guid projectId, BoardFilter filter, CancellationToken cancellationToken = default);
}
