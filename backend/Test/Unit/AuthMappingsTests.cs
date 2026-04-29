using Backend.DTOs.Users.Requests;
using Backend.Mappings;
using Backend.Models;

namespace Backend.UnitTests;

public class AuthMappingsTests
{
    [Fact]
    public void NormalizeEmail_trims_and_lowercases()
    {
        Assert.Equal("a@b.com", AuthMappings.NormalizeEmail("  A@B.COM  "));
    }

    [Fact]
    public void RegisterDto_ToNewUser_sets_normalized_email()
    {
        var dto = new RegisterDto { Email = "  User@EXAMPLE.com  ", Password = "secret12" };
        var user = dto.ToNewUser();
        Assert.Equal("user@example.com", user.Email);
    }

    [Fact]
    public void User_ToAuthResponse_maps_fields()
    {
        var user = new User { Id = 1, Email = "u@test.com", PasswordHash = "x" };
        var r = user.ToAuthResponse("jwt", "csrf");
        Assert.Equal("jwt", r.Token);
        Assert.Equal("u@test.com", r.Email);
        Assert.Equal("csrf", r.CsrfToken);
    }

    [Fact]
    public void User_ToRefreshToken_sets_hashes_and_expiry()
    {
        var user = new User { Id = 5, Email = "u@test.com", PasswordHash = "x" };
        var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var rt = user.ToRefreshToken("th", "ch", now);
        Assert.Equal(5, rt.UserId);
        Assert.Equal("th", rt.TokenHash);
        Assert.Equal("ch", rt.CsrfTokenHash);
        Assert.Equal(now, rt.CreatedAtUtc);
        Assert.Equal(now + AuthMappings.RefreshTokenLifetime, rt.ExpiresAtUtc);
    }
}
