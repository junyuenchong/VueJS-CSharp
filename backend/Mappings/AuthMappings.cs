using Backend.DTOs.Users.Requests;
using Backend.DTOs.Users.Responses;
using Backend.Models;

namespace Backend.Mappings;

/*
 * Auth mapping + normalization (email rules, access token response shape, refresh token entity creation).
 */
public static class AuthMappings
{
    public static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(30);

    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    public static User ToNewUser(this RegisterDto dto)
    {
        return new User { Email = NormalizeEmail(dto.Email) };
    }

    public static AuthResponseDto ToAuthResponse(this User user, string accessToken, string csrfToken)
    {
        return new AuthResponseDto
        {
            Token = accessToken,
            Email = user.Email,
            CsrfToken = csrfToken
        };
    }

    public static RefreshToken ToRefreshToken(this User user, string tokenHash, string csrfTokenHash, DateTime utcNow)
    {
        return new RefreshToken
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            CsrfTokenHash = csrfTokenHash,
            CreatedAtUtc = utcNow,
            ExpiresAtUtc = utcNow + RefreshTokenLifetime,
        };
    }
}
