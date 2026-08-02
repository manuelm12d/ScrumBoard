using ScrumBoard.Application.Abstractions.Persistence;
using ScrumBoard.Application.Abstractions.Security;
using ScrumBoard.Application.Common;
using ScrumBoard.Domain.Users;

namespace ScrumBoard.Application.Auth;

public sealed class AuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthService(IUserRepository users, IPasswordHasher passwordHasher, ITokenService tokenService)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var email = User.NormalizeEmail(request.Email);
        var user = await _users.GetByEmailAsync(email, cancellationToken);

        // A missing user and a wrong password produce the same error so the endpoint cannot be used
        // to enumerate registered accounts.
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash, user.PasswordSalt))
        {
            throw new InvalidCredentialsException();
        }

        var (token, expiresAt) = _tokenService.Issue(user);
        return new AuthResponse(token, expiresAt, new UserDto(user.Id, user.Name, user.Email));
    }

    public async Task<IReadOnlyList<UserDto>> ListUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _users.ListAsync(cancellationToken);
        return users.Select(u => new UserDto(u.Id, u.Name, u.Email)).ToList();
    }
}
