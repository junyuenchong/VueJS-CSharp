using Backend.DTOs.Users.Requests;
using Backend.DTOs.Users.Responses;

namespace Backend.Services.Auth;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    Task<AuthResponseDto> RefreshAsync();
    Task LogoutAsync();
}

