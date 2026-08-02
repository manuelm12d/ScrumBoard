using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScrumBoard.Domain.Boards;

namespace ScrumBoard.Infrastructure.Persistence.Configurations;

internal sealed class BoardColumnConfiguration : IEntityTypeConfiguration<BoardColumn>
{
    public void Configure(EntityTypeBuilder<BoardColumn> builder)
    {
        builder.ToTable("board_columns");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Order).IsRequired();

        builder.HasMany(c => c.Tasks)
            .WithOne(t => t.Column!)
            .HasForeignKey(t => t.ColumnId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata.FindNavigation(nameof(BoardColumn.Tasks))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        // Not unique on purpose: reordering rewrites several rows in one transaction and a unique
        // constraint would fail on the intermediate states unless it were deferrable.
        builder.HasIndex(c => new { c.ProjectId, c.Order }).HasDatabaseName("ix_board_columns_project_order");
    }
}
