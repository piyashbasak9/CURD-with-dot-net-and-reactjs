
using System.ComponentModel.DataAnnotations;

namespace TaskManager.Application.DTOs.Auth;

public class VerifyEmailDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^\d{6}$",
        ErrorMessage = "Verification code must be exactly 6 digits.")]
    public string Code { get; set; } = string.Empty;
}
