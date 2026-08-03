using Microsoft.EntityFrameworkCore;
using ScrumBoard.Application.Abstractions.Persistence;
using ScrumBoard.Application.Boards;
using ScrumBoard.Application.Tasks;

namespace ScrumBoard.Infrastructure.Persistence.Queries;

internal sealed class BoardQuery : IBoardQuery
{
    private readonly AppDbContext _context;

    public BoardQuery(AppDbContext context) => _context = context;

    public async Task<BoardDto?> GetBoardAsync(Guid projectId, BoardFilter filter, CancellationToken cancellationToken = default)
    {
        var assigneeId = filter.AssigneeId;
        var priority = filter.Priority;
        var search = string.IsNullOrWhiteSpace(filter.Search) ? null : $"%{filter.Search.Trim()}%";

        return await _context.Projects
            .AsNoTracking()
            .Where(p => p.Id == projectId)
            .Select(p => new BoardDto(
                p.Id,
                p.Name,
                p.Columns
                    .OrderBy(c => c.Order)
                    .Select(c => new BoardColumnWithTasksDto(
                        c.Id,
                        c.Name,
                        c.Order,
                        c.Tasks
                            .Where(t => (assigneeId == null || t.AssigneeId == assigneeId)
                                        && (priority == null || (int)t.Priority == priority)
                                        && (search == null || EF.Functions.ILike(t.Title, search)))
                            .OrderBy(t => t.Position)
                            .Select(t => new TaskDto(
                                t.Id,
                                t.ColumnId,
                                p.Id,
                                t.Title,
                                t.Description,
                                t.Priority,
                                t.AssigneeId,
                                t.Assignee != null ? t.Assignee.Name : null,
                                t.Position,
                                t.CreatedAtUtc))
                            .ToList()))
                    .ToList()))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
