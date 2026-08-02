using ScrumBoard.Domain.Boards;
using ScrumBoard.Domain.Common;

namespace ScrumBoard.Domain.Projects;

public class Project
{
    private readonly List<BoardColumn> _columns = new();

    private Project()
    {
    }

    public Project(
        Guid id,
        string name,
        string? description,
        DateOnly startDate,
        DateOnly? plannedEndDate,
        ProjectStatus status,
        DateTime createdAtUtc)
    {
        Id = id;
        CreatedAtUtc = createdAtUtc;
        Update(name, description, startDate, plannedEndDate, status);
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = null!;

    public string? Description { get; private set; }

    public DateOnly StartDate { get; private set; }

    public DateOnly? PlannedEndDate { get; private set; }

    public ProjectStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public IReadOnlyCollection<BoardColumn> Columns => _columns;

    public void Update(string name, string? description, DateOnly startDate, DateOnly? plannedEndDate, ProjectStatus status)
    {
        if (plannedEndDate is not null && plannedEndDate < startDate)
        {
            throw new DomainValidationException("La fecha de fin prevista no puede ser anterior a la fecha de inicio.");
        }

        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name), 150);
        Description = Guard.AgainstTooLong(description, nameof(description), 2000);
        StartDate = startDate;
        PlannedEndDate = plannedEndDate;
        Status = status;
    }
}
