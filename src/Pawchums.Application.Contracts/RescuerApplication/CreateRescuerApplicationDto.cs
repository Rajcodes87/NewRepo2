using AnimalRescueSystem.Constants;
using System.ComponentModel.DataAnnotations;

namespace AnimalRescueSystem.RescuerApplications;

public class CreateRescuerApplicationDto
{
    [Required(ErrorMessage = "Identity card picture is required.")]
    public string IdentityCardPicture { get; set; } // Base64 encoded image
}