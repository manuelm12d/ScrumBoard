using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScrumBoard.Domain.Tasks;

namespace ScrumBoard.Infrastructure.Persistence.Configurations;

internal sealed class WorkTaskConfiguration : IEntityTypeConfiguration<WorkTask>
{
    public void Configure(EntityTypeBuilder<WorkTask> builder)
    {
        builder.ToTable("tasks");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title).HasMaxLength(200).IsRequired();
        builder.Property(t => t.Description).HasMaxLength(4000);
        builder.Property(t => t.Priority).HasConversion<int>().IsRequired();
        builder.Property(t => t.CreatedAtUtc).IsRequired();
        builder.Property(t => t.UpdatedAtUtc).IsRequired();

        // numeric(18,9): fractional indexing needs exact decimals. A float would drift and two
        // midpoints could collapse onto the same value.
        builder.Property(t => t.Position).HasColumnType("numeric(18,9)").IsRequired();

        builder.HasOne(t => t.Assignee)
            .WithMany()
            .HasForeignKey(t => t.AssigneeId)
            .OnDelete(DeleteBehavior.Restrict);

        // The ordering index: every board read is "tasks of this column sorted by position", so the
        // composite index answers the filter and the sort without a sort node.
        builder.HasIndex(t => new { t.ColumnId, t.Position }).HasDatabaseName("ix_tasks_column_position");

        builder.HasIndex(t => t.AssigneeId).HasDatabaseName("ix_tasks_assignee");
    }
}
