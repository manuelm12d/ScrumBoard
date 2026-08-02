using ScrumBoard.Application.Common;
using ScrumBoard.Domain.Projects;

namespace ScrumBoard.Application.Abstractions.Persistence;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Server side paging and partial name matching; never materializes the whole table.</summary>
    Task<PagedResult<Project>> SearchAsync(string? nameFilter, int page, int pageSize, CancellationToken cancellationToken = default);

    Task AddAsync(Project project, CancellationToken cancellationToken = default);

    void Remove(Project project);
}
