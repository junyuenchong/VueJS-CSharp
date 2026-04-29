using Backend.DTOs.Users.Requests;
using Backend.DTOs.Users.Responses;

namespace Backend.Services.Auth;

/*
 * Auth business logic: credentials, token issuance/rotation, logout.
 */
public interface IAuthService
{
    /* Register: create user, persist, issue tokens. */
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);

    /* Login: verify credentials and issue tokens. */
    Task<AuthResponseDto> LoginAsync(LoginDto dto);

    /* Refresh: validate refresh cookie + CSRF header, rotate tokens. */
    Task<AuthResponseDto> RefreshAsync();

    /* Logout: revoke refresh token (if present) and clear auth cookies. */
    Task LogoutAsync();
}
