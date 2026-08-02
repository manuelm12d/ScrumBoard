using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScrumBoard.Domain.Projects;

namespace ScrumBoard.Infrastructure.Persistence.Configurations;

internal sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("projects");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).HasMaxLength(150).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(2000);
        builder.Property(p => p.StartDate).HasColumnType("date").IsRequired();
        builder.Property(p => p.PlannedEndDate).HasColumnType("date");
        builder.Property(p => p.Status).HasConversion<int>().IsRequired();
        builder.Property(p => p.CreatedAtUtc).IsRequired();

        builder.HasMany(p => p.Columns)
            .WithOne(c => c.Project!)
            .HasForeignKey(c => c.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata.FindNavigation(nameof(Project.Columns))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        // Trigram index: the list filters by partial name with ILIKE '%x%', which a btree index cannot
        // serve. GIN + gin_trgm_ops keeps that search index-backed as the table grows.
        builder.HasIndex(p => p.Name)
            .HasDatabaseName("ix_projects_name_trgm")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");
    }
}
