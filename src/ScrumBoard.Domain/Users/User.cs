using ScrumBoard.Domain.Common;

namespace ScrumBoard.Domain.Users;

public class User
{
    private User()
    {
    }

    public User(Guid id, string name, string email, string passwordHash, string passwordSalt, DateTime createdAtUtc)
    {
        Id = id;
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name), 120);
        Email = NormalizeEmail(email);
        PasswordHash = Guard.AgainstNullOrWhiteSpace(passwordHash, nameof(passwordHash), 512);
        PasswordSalt = Guard.AgainstNullOrWhiteSpace(passwordSalt, nameof(passwordSalt), 512);
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = null!;

    /// <summary>Stored normalized (lower case) so the unique index also enforces case-insensitive uniqueness.</summary>
    public string Email { get; private set; } = null!;

    public string PasswordHash { get; private set; } = null!;

    /// <summary>Per-user random salt. The pepper is never persisted; it comes from configuration.</summary>
    public string PasswordSalt { get; private set; } = null!;

    public DateTime CreatedAtUtc { get; private set; }

    public static string NormalizeEmail(string email)
    {
        var normalized = Guard.AgainstNullOrWhiteSpace(email, nameof(email), 200).ToLowerInvariant();
        if (!normalized.Contains('@') || normalized.StartsWith('@') || normalized.EndsWith('@'))
        {
            throw new DomainValidationException("El correo electrónico no tiene un formato válido.");
        }

        return normalized;
    }
}
