namespace ScrumBoard.Infrastructure.Security;

public sealed class PasswordHashingOptions
{
    public const string SectionName = "Security";

    /// <summary>
    /// Application wide secret mixed into every password before hashing. It lives in configuration and
    /// never in the database, so a dump of the users table is not enough to mount an offline attack.
    /// </summary>
    public string PasswordPepper { get; set; } = string.Empty;
}
