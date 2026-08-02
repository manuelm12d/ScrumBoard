using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScrumBoard.Domain.Users;

namespace ScrumBoard.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Name).HasMaxLength(120).IsRequired();
        builder.Property(u => u.Email).HasMaxLength(200).IsRequired();
        builder.Property(u => u.PasswordHash).HasMaxLength(512).IsRequired();
        builder.Property(u => u.PasswordSalt).HasMaxLength(512).IsRequired();
        builder.Property(u => u.CreatedAtUtc).IsRequired();

        // Emails are normalized to lower case by the domain, so a plain unique index is enough to make
        // uniqueness case insensitive without a functional index.
        builder.HasIndex(u => u.Email).IsUnique().HasDatabaseName("ix_users_email");

        builder.HasData(
            new
            {
                Id = SeedData.AdminUserId,
                Name = "Admin IdeasGroup",
                Email = "admin@ideasgroup.test",
                PasswordHash = SeedData.AdminPasswordHash,
                PasswordSalt = SeedData.AdminPasswordSalt,
                CreatedAtUtc = SeedData.SeedInstantUtc
            },
            new
            {
                Id = SeedData.DeveloperUserId,
                Name = "Dev IdeasGroup",
                Email = "dev@ideasgroup.test",
                PasswordHash = SeedData.DeveloperPasswordHash,
                PasswordSalt = SeedData.DeveloperPasswordSalt,
                CreatedAtUtc = SeedData.SeedInstantUtc
            });
    }
}
