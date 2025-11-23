using System.ComponentModel.DataAnnotations;

namespace Pawchums.Identity;

public class VerifyEmailDto
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Verification code is required.")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Verification code must be 6 digits.")]
    public string Code { get; set; }
}
