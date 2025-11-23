using System.ComponentModel.DataAnnotations;

namespace Pawchums.Identity;

public class ForgotPasswordDto
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; }
}
