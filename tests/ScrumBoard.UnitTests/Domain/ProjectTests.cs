using ScrumBoard.Domain.Common;
using ScrumBoard.Domain.Projects;

namespace ScrumBoard.UnitTests.Domain;

public class ProjectTests
{
    [Fact]
    public void Create_WithEndDateBeforeStartDate_IsRejected()
    {
        Assert.Throws<DomainValidationException>(() => new Project(
            Guid.NewGuid(),
            "Sprint 1",
            null,
            new DateOnly(2026, 5, 10),
            new DateOnly(2026, 5, 1),
            ProjectStatus.Active,
            DateTime.UtcNow));
    }

    [Fact]
    public void Create_TrimsTheName()
    {
        var project = new Project(
            Guid.NewGuid(),
            "  Plataforma Scrum  ",
            null,
            new DateOnly(2026, 1, 1),
            null,
            ProjectStatus.Planned,
            DateTime.UtcNow);

        Assert.Equal("Plataforma Scrum", project.Name);
    }
}
