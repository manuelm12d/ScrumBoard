namespace ScrumBoard.Infrastructure.Persistence;

/// <summary>
/// Values consumed by the seed migration. They are constants because EF Core requires deterministic
/// data in HasData: a Guid.NewGuid() or DateTime.UtcNow here would make every later "migrations add"
/// produce a spurious diff.
/// </summary>
internal static class SeedData
{
    internal static readonly DateTime SeedInstantUtc = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    internal static readonly Guid AdminUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    internal static readonly Guid DeveloperUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    /// <summary>
    /// Hashes generated with PBKDF2-HMAC-SHA256, 100k iterations and the pepper shipped in .env.example
    /// (Security__PasswordPepper=dev-pepper-ideasgroup-2026). Changing the pepper invalidates these
    /// credentials, which is exactly the point of a pepper.
    /// </summary>
    internal const string AdminPasswordHash = "lbK+fArFcTETf7sxjYqOIbCsAHv7v60EvV3J6KdOkbY=";

    internal const string AdminPasswordSalt = "tEaLkTcDLCcO9Z5OmbBpDg==";

    internal const string DeveloperPasswordHash = "oN1iWW+uYT7u9G0beFtDd7qgkwMgVI6F8lb1DB1wCyY=";

    internal const string DeveloperPasswordSalt = "C3tVEoyzsAjrVhjc2Mw7cQ==";
}
