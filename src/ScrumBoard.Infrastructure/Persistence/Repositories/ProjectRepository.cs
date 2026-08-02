using Microsoft.EntityFrameworkCore;
using ScrumBoard.Application.Abstractions.Persistence;
using ScrumBoard.Application.Common;
using ScrumBoard.Domain.Projects;

namespace ScrumBoard.Infrastructure.Persistence.Repositories;

internal sealed class ProjectRepository : IProjectRepository
{
    private readonly AppDbContext _context;

    public ProjectRepository(AppDbContext context) => _context = context;

    public Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.Projects.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.Projects.AnyAsync(p => p.Id == id, cancellationToken);

    public async Task<PagedResult<Project>> SearchAsync(
        string? nameFilter,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Projects.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(nameFilter))
        {
            // EF.Functions.ILike maps to the native ILIKE operator, which the pg_trgm GIN index can
            // serve. ToLower().Contains() would force a sequential scan instead.
            var pattern = $"%{Escape(nameFilter.Trim())}%";
            query = query.Where(p => EF.Functions.ILike(p.Name, pattern));
        }

        var total = await query.LongCountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(p => p.CreatedAtUtc)
            .ThenBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Project>(items, page, pageSize, total);
    }

    public async Task AddAsync(Project project, CancellationToken cancellationToken = default)
        => await _context.Projects.AddAsync(project, cancellationToken);

    public void Remove(Project project) => _context.Projects.Remove(project);

    /// <summary>Neutralizes the LIKE wildcards a user could type so the filter cannot be abused.</summary>
    private static string Escape(string value)
        => value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}
