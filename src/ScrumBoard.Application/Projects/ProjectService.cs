using ScrumBoard.Application.Abstractions.Persistence;
using ScrumBoard.Application.Abstractions.Time;
using ScrumBoard.Application.Common;
using ScrumBoard.Domain.Projects;

namespace ScrumBoard.Application.Projects;

public sealed class ProjectService
{
    private const int MaxPageSize = 100;

    private readonly IProjectRepository _projects;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public ProjectService(IProjectRepository projects, IUnitOfWork unitOfWork, IClock clock)
    {
        _projects = projects;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<PagedResult<ProjectDto>> SearchAsync(ProjectQuery query, CancellationToken cancellationToken = default)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);

        var result = await _projects.SearchAsync(query.Name, page, pageSize, cancellationToken);
        return new PagedResult<ProjectDto>(
            result.Items.Select(Map).ToList(),
            result.Page,
            result.PageSize,
            result.TotalItems);
    }

    public async Task<ProjectDto> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var project = await _projects.GetByIdAsync(id, cancellationToken)
                      ?? throw new NotFoundException("el proyecto", id);
        return Map(project);
    }

    public async Task<ProjectDto> CreateAsync(SaveProjectRequest request, CancellationToken cancellationToken = default)
    {
        var project = new Project(
            Guid.NewGuid(),
            request.Name,
            request.Description,
            request.StartDate,
            request.PlannedEndDate,
            request.Status,
            _clock.UtcNow);

        await _projects.AddAsync(project, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(project);
    }

    public async Task<ProjectDto> UpdateAsync(Guid id, SaveProjectRequest request, CancellationToken cancellationToken = default)
    {
        var project = await _projects.GetByIdAsync(id, cancellationToken)
                      ?? throw new NotFoundException("el proyecto", id);

        project.Update(request.Name, request.Description, request.StartDate, request.PlannedEndDate, request.Status);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(project);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var project = await _projects.GetByIdAsync(id, cancellationToken)
                      ?? throw new NotFoundException("el proyecto", id);

        _projects.Remove(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static ProjectDto Map(Project project) => new(
        project.Id,
        project.Name,
        project.Description,
        project.StartDate,
        project.PlannedEndDate,
        project.Status,
        project.CreatedAtUtc);
}
