using ScrumBoard.Domain.Users;

namespace ScrumBoard.Application.Abstractions.Security;

public interface ITokenService
{
    (string Token, DateTime ExpiresAtUtc) Issue(User user);
}
