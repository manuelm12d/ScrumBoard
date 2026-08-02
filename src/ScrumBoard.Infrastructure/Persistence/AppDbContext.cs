using Microsoft.EntityFrameworkCore;
using ScrumBoard.Application.Abstractions.Persistence;
using ScrumBoard.Domain.Boards;
using ScrumBoard.Domain.Projects;
using ScrumBoard.Domain.Tasks;
using ScrumBoard.Domain.Users;
using ScrumBoard.Infrastructure.Persistence.Configurations;

namespace ScrumBoard.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext, IUnitOfWork
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Project> Projects => Set<Project>();

    public DbSet<BoardColumn> BoardColumns => Set<BoardColumn>();

    public DbSet<WorkTask> Tasks => Set<WorkTask>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("pg_trgm");

        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new ProjectConfiguration());
        modelBuilder.ApplyConfiguration(new BoardColumnConfiguration());
        modelBuilder.ApplyConfiguration(new WorkTaskConfiguration());
    }
}
