using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

public class User
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

