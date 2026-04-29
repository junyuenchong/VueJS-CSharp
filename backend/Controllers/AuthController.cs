using Backend.DTOs.Users.Requests;
using Backend.DTOs.Users.Responses;
using Backend.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
/*
 * Authentication endpoints (register/login/refresh/logout).
 * Uses JWT access token + refresh-token cookie with CSRF header protection.
 */
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("register")]
    [EnableRateLimiting("auth")]
    /*
     * Create a new user account and issue tokens.
     */
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var result = await _authService.RegisterAsync(dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register user.");
            return Problem("An unexpected error occurred during registration.");
        }
    }

    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    /*
     * Validate credentials and issue tokens.
     */
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var result = await _authService.LoginAsync(dto);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to login user.");
            return Problem("An unexpected error occurred during login.");
        }
    }

    [HttpPost("refresh")]
    [EnableRateLimiting("auth")]
    /*
     * Rotate refresh token and return a new access token (requires CSRF header).
     */
    public async Task<ActionResult<AuthResponseDto>> Refresh()
    {
        try
        {
            var result = await _authService.RefreshAsync();
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (InvalidOperationException ex) when (ex.Message == "Invalid CSRF token")
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh token.");
            return Problem("An unexpected error occurred while refreshing authentication.");
        }
    }

    [HttpPost("logout")]
    [EnableRateLimiting("auth")]
    /*
     * Revoke the current refresh token (requires CSRF header) and clear auth cookies.
     */
    public async Task<IActionResult> Logout()
    {
        try
        {
            await _authService.LogoutAsync();
            return NoContent();
        }
        catch (InvalidOperationException ex) when (ex.Message == "Invalid CSRF token")
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to logout user.");
            return Problem("An unexpected error occurred during logout.");
        }
    }
}

