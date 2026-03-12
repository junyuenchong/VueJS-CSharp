using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Users.Requests;

public class RegisterDto
{
    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
}

