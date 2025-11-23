using System.ComponentModel.DataAnnotations;

namespace Pawchums.Identity;

public class RegisterRescuerDto
{
    [Required(ErrorMessage = "Username is required.")]
    [StringLength(256, ErrorMessage = "Username must not exceed 256 characters.")]
    public string UserName { get; set; }

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    [StringLength(256, ErrorMessage = "Email must not exceed 256 characters.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(128, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 128 characters.")]
    public string Password { get; set; }

    [StringLength(64, ErrorMessage = "Name must not exceed 64 characters.")]
    public string Name { get; set; }

    [StringLength(64, ErrorMessage = "Surname must not exceed 64 characters.")]
    public string Surname { get; set; }

    [Phone(ErrorMessage = "Invalid phone number format.")]
    [StringLength(16, ErrorMessage = "Phone number must not exceed 16 characters.")]
    public string PhoneNumber { get; set; }
}
