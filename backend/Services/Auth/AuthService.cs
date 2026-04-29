using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Backend.Data;
using Backend.DTOs.Users.Requests;
using Backend.DTOs.Users.Responses;
using Backend.Mappings;
using Backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace Backend.Services.Auth;

/*
 * Handles issuing JWT access tokens and manages refresh token rotation via HttpOnly cookies.
 * - Enforces CSRF for cookie-based operations (refresh/logout) via a header token.
 * - Ensures secure authentication flows for all clients.
 */
public class AuthService : IAuthService
{
    // Dependencies and constants
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    private readonly IHostEnvironment _env;
    private readonly IHttpContextAccessor _http;
    private readonly PasswordHasher<User> _hasher = new();

    private const string RefreshCookieName = "refresh_token";
    private const string CsrfHeaderName = "X-CSRF-TOKEN";

    // Constructor to inject dependencies
    public AuthService(AppDbContext db, IConfiguration config, IHostEnvironment env, IHttpContextAccessor http)
    {
        _db = db;
        _config = config;
        _env = env;
        _http = http;
    }

    /* Register a new user and return access and CSRF tokens */
    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        // Map DTO to user entity
        var user = dto.ToNewUser();

        // Ensure email uniqueness
        var exists = await _db.Users.AsNoTracking().AnyAsync(u => u.Email == user.Email);
        if (exists)
            throw new InvalidOperationException("Email already registered");

        // Hash password and persist user
        user.PasswordHash = _hasher.HashPassword(user, dto.Password);
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // Issue tokens on registration
        var csrfToken = await IssueRefreshTokenAsync(user);
        var token = CreateAccessToken(user);

        // Combine user data and tokens in response
        return user.ToAuthResponse(token, csrfToken);
    }

    /*  Login with email and password, return auth tokens */
    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        // Normalize and look up email
        var email = AuthMappings.NormalizeEmail(dto.Email);
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
            throw new UnauthorizedAccessException("Invalid email or password");

        // Confirm password hash
        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
        if (result == PasswordVerificationResult.Failed)
            throw new UnauthorizedAccessException("Invalid email or password");

        // Issue tokens after login
        var csrfToken = await IssueRefreshTokenAsync(user);
        var token = CreateAccessToken(user);

        // Return combined authentication response
        return user.ToAuthResponse(token, csrfToken);
    }

    /*  Refresh tokens via cookie+header and rotate refresh token */
    public async Task<AuthResponseDto> RefreshAsync()
    {
        // Retrieve request context
        var context = _http.HttpContext ?? throw new InvalidOperationException("No HTTP context");
        // Extract refresh token from cookie
        var refreshToken = context.Request.Cookies[RefreshCookieName];
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new UnauthorizedAccessException("Missing refresh token");

        // Prepare hash for DB lookup
        var tokenHash = HashToken(refreshToken);

        // Query for corresponding refresh token and user
        var stored = await _db.RefreshTokens
            .AsNoTracking()
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);

        // Validate token presence and activity
        if (stored == null || !stored.IsActive)
            throw new UnauthorizedAccessException("Invalid refresh token");

        // Verify CSRF token in header matches stored value
        if (!IsValidCsrfTokenFor(stored))
            throw new InvalidOperationException("Invalid CSRF token");

        // Revoke token and rotate to new tokens
        await _db.RefreshTokens
            .Where(rt => rt.Id == stored.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(rt => rt.RevokedAtUtc, DateTime.UtcNow));

        // Issue new set of tokens
        var csrfToken = await IssueRefreshTokenAsync(stored.User);
        var access = CreateAccessToken(stored.User);

        // Respond with refreshed authentication info
        return stored.User.ToAuthResponse(access, csrfToken);
    }

    /* Logout by revoking refresh token and clearing cookies */
    public async Task LogoutAsync()
    {
        // Get HTTP context
        var context = _http.HttpContext ?? throw new InvalidOperationException("No HTTP context");
        // Attempt to read refresh token cookie
        var refreshToken = context.Request.Cookies[RefreshCookieName];

        // Only process if refresh token exists
        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            // Hash token for lookup and get stored item
            var tokenHash = HashToken(refreshToken);
            var stored = await _db.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);

            if (stored != null)
            {
                // Ensure CSRF header token matches stored hash
                if (!IsValidCsrfTokenFor(stored))
                    throw new InvalidOperationException("Invalid CSRF token");

                // Revocation performed only if not already revoked
                if (stored.RevokedAtUtc == null)
                {
                    stored.RevokedAtUtc = DateTime.UtcNow;
                    await _db.SaveChangesAsync();
                }
            }
        }

        // Always clear cookies regardless of token status
        ClearAuthCookies();
    }

    /* Creates a short-lived JWT access token (for header usage) */
    private string CreateAccessToken(User user)
    {
        // Prepare signing key and credentials
        var key = new SymmetricSecurityKey(GetJwtKeyBytes());
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Compose token claims
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
        };

        // Build JWT structure and set expiration
        var token = new JwtSecurityToken(
            issuer: null,
            audience: null,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: creds
        );

        // Serialize JWT to string
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /* Store new hashed refresh & CSRF tokens, set HttpOnly cookie */
    private async Task<string> IssueRefreshTokenAsync(User user)
    {
        // Get HTTP context for response
        var context = _http.HttpContext ?? throw new InvalidOperationException("No HTTP context");

        // Create secure random tokens
        var raw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var now = DateTime.UtcNow;
        var csrf = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

        // Persist hashed tokens in DB
        _db.RefreshTokens.Add(user.ToRefreshToken(HashToken(raw), HashToken(csrf), now));
        await _db.SaveChangesAsync();

        // Configure cookie for refresh token
        var refreshCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = !_env.IsDevelopment(),
            SameSite = SameSiteMode.Lax,
            Path = "/api/Auth",
            Expires = DateTimeOffset.UtcNow.Add(AuthMappings.RefreshTokenLifetime)
        };

        // Attach refresh token cookie to response
        context.Response.Cookies.Append(RefreshCookieName, raw, refreshCookieOptions);
        return csrf;
    }

    /* Clear HttpOnly refresh token cookie from client */
    private void ClearAuthCookies()
    {
        // Delete refresh token cookie explicitly
        var context = _http.HttpContext ?? throw new InvalidOperationException("No HTTP context");
        context.Response.Cookies.Delete(RefreshCookieName, new CookieOptions { Path = "/api/Auth" });
    }

    /* Verify incoming CSRF header token against hash in DB */
    private bool IsValidCsrfTokenFor(RefreshToken stored)
    {
        // Get HTTP context and header value
        var context = _http.HttpContext ?? throw new InvalidOperationException("No HTTP context");
        if (!context.Request.Headers.TryGetValue(CsrfHeaderName, out var headerValues)) return false;
        var header = headerValues.ToString();
        if (string.IsNullOrWhiteSpace(header)) return false;

        // Compare hashes for validity
        return string.Equals(HashToken(header), stored.CsrfTokenHash, StringComparison.Ordinal);
    }

    /* SHA-256 one-way hash for tokens before DB persistence */
    private static string HashToken(string raw)
    {
        var bytes = Encoding.UTF8.GetBytes(raw);
        return Convert.ToHexString(SHA256.HashData(bytes));
    }

    /* Get HMAC signing key bytes, pad or hash to secure length */
    private byte[] GetJwtKeyBytes()
    {
        var secret = _config["JWT_SECRET"] ?? "dev-only-secret-change-me";
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        if (keyBytes.Length < 32)
            keyBytes = SHA256.HashData(keyBytes);
        return keyBytes;
    }
}
