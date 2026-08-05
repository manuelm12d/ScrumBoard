using Microsoft.EntityFrameworkCore;
using ScrumBoard.Application.Abstractions.Reporting;
using ScrumBoard.Application.Abstractions.Time;
using ScrumBoard.Application.Boards;
using ScrumBoard.Application.Reports;

namespace ScrumBoard.Infrastructure.Persistence.Queries;

/// <summary>
/// One round trip feeds every export format: the header and the flattened task rows travel together
/// and the exporters never touch the database.
/// </summary>
internal sealed class ProjectReportQuery : IProjectReportQuery
{
    private readonly AppDbContext _context;
    private readonly IClock _clock;

    public ProjectReportQuery(AppDbContext context, IClock clock)
    {
        _context = context;
        _clock = clock;
    }

    public async Task<ProjectReportDto?> GetAsync(Guid projectId, BoardFilter filter, CancellationToken cancellationToken = default)
    {
        var assigneeId = filter.AssigneeId;
        var priority = filter.Priority;
        var search = string.IsNullOrWhiteSpace(filter.Search) ? null : $"%{filter.Search.Trim()}%";

        var data = await _context.Projects
            .AsNoTracking()
            .Where(p => p.Id == projectId)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Description,
                p.StartDate,
                p.PlannedEndDate,
                p.Status,
                Rows = p.Columns
                    .SelectMany(c => c.Tasks
                        .Where(t => (assigneeId == null || t.AssigneeId == assigneeId)
                                    && (priority == null || (int)t.Priority == priority)
                                    && (search == null || EF.Functions.ILike(t.Title, search)))
                        .Select(t => new
                        {
                            ColumnOrder = c.Order,
                            ColumnName = c.Name,
                            t.Position,
                            t.Title,
                            AssigneeName = t.Assignee != null ? t.Assignee.Name : null,
                            t.Priority,
                            t.CreatedAtUtc
                        }))
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (data is null)
        {
            return null;
        }

        // Sorting happens once the flattened rows are in memory: SelectMany does not guarantee the
        // order of the nested projection in SQL.
        var rows = data.Rows
            .OrderBy(r => r.ColumnOrder)
            .ThenBy(r => r.Position)
            .Select(r => new ProjectReportRow(
                r.Title,
                r.ColumnName,
                r.AssigneeName ?? "Sin asignar",
                r.Priority,
                r.CreatedAtUtc))
            .ToList();

        return new ProjectReportDto(
            data.Id,
            data.Name,
            data.Description,
            data.StartDate,
            data.PlannedEndDate,
            data.Status,
            _clock.UtcNow,
            rows);
    }
}
