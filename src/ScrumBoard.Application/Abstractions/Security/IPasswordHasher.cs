namespace ScrumBoard.Application.Abstractions.Security;

public interface IPasswordHasher
{
    (string Hash, string Salt) Hash(string password);

    bool Verify(string password, string hash, string salt);
}
