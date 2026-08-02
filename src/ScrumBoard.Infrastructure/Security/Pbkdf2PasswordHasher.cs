using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using ScrumBoard.Application.Abstractions.Security;

namespace ScrumBoard.Infrastructure.Security;

/// <summary>
/// PBKDF2-HMAC-SHA256 with a per-user random salt (stored) and an application pepper (configured).
/// PBKDF2 ships with the framework, so there is no third party dependency in the credential path.
/// </summary>
public sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    public const int Iterations = 100_000;
    private const int SaltSize = 16;
    private const int KeySize = 32;

    private readonly string _pepper;

    public Pbkdf2PasswordHasher(IOptions<PasswordHashingOptions> options)
    {
        _pepper = options.Value.PasswordPepper;
        if (string.IsNullOrWhiteSpace(_pepper))
        {
            throw new InvalidOperationException(
                "Security__PasswordPepper no está configurado; la API no puede validar contraseñas.");
        }
    }

    public (string Hash, string Salt) Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Derive(password, salt);
        return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
    }

    public bool Verify(string password, string hash, string salt)
    {
        byte[] expected;
        byte[] saltBytes;

        try
        {
            expected = Convert.FromBase64String(hash);
            saltBytes = Convert.FromBase64String(salt);
        }
        catch (FormatException)
        {
            return false;
        }

        var actual = Derive(password, saltBytes);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }

    private byte[] Derive(string password, byte[] salt)
    {
        var material = Encoding.UTF8.GetBytes(password + _pepper);
        return Rfc2898DeriveBytes.Pbkdf2(material, salt, Iterations, HashAlgorithmName.SHA256, KeySize);
    }
}
