using AnimalRescueSystem.Constants;
using System;
using System.ComponentModel.DataAnnotations;

namespace AnimalRescueSystem.RescueInitiations;

public class CreateRescueInitiationDto
{
    [Required(ErrorMessage = "Request Rescue Id is required.")]
    public Guid RequestRescueId { get; set; }

    [StringLength(RescueInitiationConsts.MaxLength.Notes, ErrorMessage = "Notes must not exceed {1} characters.")]
    public string? Notes { get; set; }
}