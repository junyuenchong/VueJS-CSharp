using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Backend.Data;
using Backend.DTOs.Users.Requests;
using Backend.DTOs.Users.Responses;
using Backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace Backend.Services.Auth;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    private readonly IHostEnvironment _env;
    private readonly IHttpContextAccessor _http;
    private readonly PasswordHasher<User> _hasher = new();

    private const string RefreshCookieName = "refresh_token";
    private const string CsrfHeaderName = "X-CSRF-TOKEN";

    public AuthService(AppDbContext db, IConfiguration config, IHostEnvironment env, IHttpContextAccessor http)
    {
        _db = db;
        _config = config;
        _env = env;
        _http = http;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var exists = await _db.Users.AsNoTracking().AnyAsync(u => u.Email == email);
        if (exists) throw new InvalidOperationException("Email already registered");

        var user = new User { Email = email };
        user.PasswordHash = _hasher.HashPassword(user, dto.Password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var csrfToken = await IssueRefreshTokenAsync(user);
        var token = CreateAccessToken(user);
        return new AuthResponseDto { Token = token, Email = user.Email, CsrfToken = csrfToken };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) throw new UnauthorizedAccessException("Invalid email or password");

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
        if (result == PasswordVerificationResult.Failed)
            throw new UnauthorizedAccessException("Invalid email or password");

        var csrfToken = await IssueRefreshTokenAsync(user);
        var token = CreateAccessToken(user);
        return new AuthResponseDto { Token = token, Email = user.Email, CsrfToken = csrfToken };
    }

    public async Task<AuthResponseDto> RefreshAsync()
    {
        var context = _http.HttpContext ?? throw new InvalidOperationException("No HTTP context");

        var refreshToken = context.Request.Cookies[RefreshCookieName];
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new UnauthorizedAccessException("Missing refresh token");

        var tokenHash = HashToken(refreshToken);
        var stored = await _db.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);

        if (stored == null || !stored.IsActive)
            throw new UnauthorizedAccessException("Invalid refresh token");

        if (!IsValidCsrfTokenFor(stored))
            throw new InvalidOperationException("Invalid CSRF token");

        stored.RevokedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var csrfToken = await IssueRefreshTokenAsync(stored.User);
        var access = CreateAccessToken(stored.User);
        return new AuthResponseDto { Token = access, Email = stored.User.Email, CsrfToken = csrfToken };
    }

    public async Task LogoutAsync()
    {
        var context = _http.HttpContext ?? throw new InvalidOperationException("No HTTP context");

        var refreshToken = context.Request.Cookies[RefreshCookieName];
        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            var tokenHash = HashToken(refreshToken);
            var stored = await _db.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);
            if (stored != null)
            {
                if (!IsValidCsrfTokenFor(stored))
                    throw new InvalidOperationException("Invalid CSRF token");

                if (stored.RevokedAtUtc == null)
                {
                    stored.RevokedAtUtc = DateTime.UtcNow;
                    await _db.SaveChangesAsync();
                }
            }
        }

        ClearAuthCookies();
    }

    private string CreateAccessToken(User user)
    {
        var key = new SymmetricSecurityKey(GetJwtKeyBytes());
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
        };

        var token = new JwtSecurityToken(
            issuer: null,
            audience: null,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<string> IssueRefreshTokenAsync(User user)
    {
        var context = _http.HttpContext ?? throw new InvalidOperationException("No HTTP context");

        var raw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var now = DateTime.UtcNow;
        var csrf = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = HashToken(raw),
            CsrfTokenHash = HashToken(csrf),
            CreatedAtUtc = now,
            ExpiresAtUtc = now.AddDays(30),
        });
        await _db.SaveChangesAsync();

        var refreshCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = !_env.IsDevelopment(),
            SameSite = SameSiteMode.Lax,
            Path = "/api/Auth",
            Expires = DateTimeOffset.UtcNow.AddDays(30)
        };

        context.Response.Cookies.Append(RefreshCookieName, raw, refreshCookieOptions);
        return csrf;
    }

    private void ClearAuthCookies()
    {
        var context = _http.HttpContext ?? throw new InvalidOperationException("No HTTP context");
        context.Response.Cookies.Delete(RefreshCookieName, new CookieOptions { Path = "/api/Auth" });
    }

    private bool IsValidCsrfTokenFor(RefreshToken stored)
    {
        var context = _http.HttpContext ?? throw new InvalidOperationException("No HTTP context");

        if (!context.Request.Headers.TryGetValue(CsrfHeaderName, out var headerValues)) return false;
        var header = headerValues.ToString();
        if (string.IsNullOrWhiteSpace(header)) return false;

        return string.Equals(HashToken(header), stored.CsrfTokenHash, StringComparison.Ordinal);
    }

    private static string HashToken(string raw)
    {
        var bytes = Encoding.UTF8.GetBytes(raw);
        return Convert.ToHexString(SHA256.HashData(bytes));
    }

    private byte[] GetJwtKeyBytes()
    {
        var secret = _config["JWT_SECRET"] ?? "dev-only-secret-change-me";
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        if (keyBytes.Length < 32)
            keyBytes = SHA256.HashData(keyBytes);
        return keyBytes;
    }
}

