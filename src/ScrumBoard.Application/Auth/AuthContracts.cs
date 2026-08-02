namespace ScrumBoard.Application.Auth;

public sealed record LoginRequest(string Email, string Password);

public sealed record UserDto(Guid Id, string Name, string Email);

public sealed record AuthResponse(string Token, DateTime ExpiresAtUtc, UserDto User);
