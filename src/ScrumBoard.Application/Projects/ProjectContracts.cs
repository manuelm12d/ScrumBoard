using ScrumBoard.Domain.Projects;

namespace ScrumBoard.Application.Projects;

public sealed record ProjectDto(
    Guid Id,
    string Name,
    string? Description,
    DateOnly StartDate,
    DateOnly? PlannedEndDate,
    ProjectStatus Status,
    DateTime CreatedAtUtc);

public sealed record SaveProjectRequest(
    string Name,
    string? Description,
    DateOnly StartDate,
    DateOnly? PlannedEndDate,
    ProjectStatus Status);

public sealed record ProjectQuery(string? Name, int Page = 1, int PageSize = 10);
