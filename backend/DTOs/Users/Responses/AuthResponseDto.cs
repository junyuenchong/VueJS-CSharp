namespace Backend.DTOs.Users.Responses;

public class AuthResponseDto
{
    public required string Token { get; init; }
    public required string Email { get; init; }
    public required string CsrfToken { get; init; }
}

