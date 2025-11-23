using AnimalRescueSystem.Constants;
using System.ComponentModel.DataAnnotations;

namespace AnimalRescueSystem.RescueInitiations;

public class UpdateRescueInitiationDto
{
    [StringLength(RescueInitiationConsts.MaxLength.Notes, ErrorMessage = "Notes must not exceed {1} characters.")]
    public string? Notes { get; set; }
}