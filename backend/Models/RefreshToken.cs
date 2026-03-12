namespace Backend.Models;

public class RefreshToken
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    // Hash of the refresh token (never store raw value)
    public string TokenHash { get; set; } = string.Empty;

    // Hash of the CSRF token used for refresh/logout
    public string CsrfTokenHash { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAtUtc { get; set; }

    public DateTime? RevokedAtUtc { get; set; }

    public bool IsActive => RevokedAtUtc == null && DateTime.UtcNow < ExpiresAtUtc;
}

