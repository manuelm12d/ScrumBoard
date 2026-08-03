using Microsoft.Extensions.Options;
using ScrumBoard.Application.Abstractions.Security;
using ScrumBoard.Application.Auth;
using ScrumBoard.Application.Common;
using ScrumBoard.Domain.Users;
using ScrumBoard.Infrastructure.Persistence;
using ScrumBoard.Infrastructure.Security;
using ScrumBoard.UnitTests.Fakes;

namespace ScrumBoard.UnitTests.Security;

public class PasswordHashingTests
{
    private const string SeedPepper = "dev-pepper-ideasgroup-2026";

    private static Pbkdf2PasswordHasher CreateHasher(string pepper = SeedPepper)
        => new(Options.Create(new PasswordHashingOptions { PasswordPepper = pepper }));

    [Fact]
    public void Hash_ProducesADifferentSaltEveryTime()
    {
        var hasher = CreateHasher();

        var first = hasher.Hash("Admin123*");
        var second = hasher.Hash("Admin123*");

        Assert.NotEqual(first.Salt, second.Salt);
        Assert.NotEqual(first.Hash, second.Hash);
        Assert.True(hasher.Verify("Admin123*", first.Hash, first.Salt));
    }

    [Fact]
    public void Verify_WithTheWrongPassword_Fails()
    {
        var hasher = CreateHasher();
        var (hash, salt) = hasher.Hash("Admin123*");

        Assert.False(hasher.Verify("admin123*", hash, salt));
    }

    /// <summary>
    /// Guards the seed migration: if the hashes or the documented pepper ever drift apart, the
    /// preloaded users would silently stop being able to log in.
    /// </summary>
    [Fact]
    public void SeededCredentials_MatchTheDocumentedPepper()
    {
        var hasher = CreateHasher();

        Assert.True(hasher.Verify("Admin123*", SeedData.AdminPasswordHash, SeedData.AdminPasswordSalt));
        Assert.True(hasher.Verify("Dev123*", SeedData.DeveloperPasswordHash, SeedData.DeveloperPasswordSalt));
    }

    [Fact]
    public void Verify_WithADifferentPepper_Fails()
    {
        Assert.False(CreateHasher("otro-pepper")
            .Verify("Admin123*", SeedData.AdminPasswordHash, SeedData.AdminPasswordSalt));
    }

    [Fact]
    public async Task LoginAsync_WithAnUnknownEmail_ReportsTheSameErrorAsAWrongPassword()
    {
        var users = new FakeUserRepository();
        var hasher = CreateHasher();
        var (hash, salt) = hasher.Hash("Admin123*");
        users.Items.Add(new User(Guid.NewGuid(), "Admin", "admin@ideasgroup.test", hash, salt, DateTime.UtcNow));

        var service = new AuthService(users, hasher, new StubTokenService());

        await Assert.ThrowsAsync<InvalidCredentialsException>(
            () => service.LoginAsync(new LoginRequest("desconocido@ideasgroup.test", "Admin123*")));
        await Assert.ThrowsAsync<InvalidCredentialsException>(
            () => service.LoginAsync(new LoginRequest("admin@ideasgroup.test", "incorrecta")));
    }

    private sealed class StubTokenService : ITokenService
    {
        public (string Token, DateTime ExpiresAtUtc) Issue(User user) => ("token", DateTime.UtcNow.AddHours(1));
    }
}
