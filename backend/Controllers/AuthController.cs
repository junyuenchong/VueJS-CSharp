using Backend.DTOs.Users.Requests;
using Backend.DTOs.Users.Responses;
using Backend.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [EnableRateLimiting("auth")]
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
    }

    [HttpPost("login")]
    [EnableRateLimiting("auth")]
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
    }

    [HttpPost("refresh")]
    [EnableRateLimiting("auth")]
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
    }

    [HttpPost("logout")]
    [EnableRateLimiting("auth")]
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
    }
}

