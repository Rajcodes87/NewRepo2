using AnimalRescueSystem.Constants;
using System;
using System.ComponentModel.DataAnnotations;

namespace Pawchums.RescueCompletions;

public class CreateRescueCompletionDto
{
    [Required(ErrorMessage = "Request Rescue Id is required.")]
    public Guid RequestRescueId { get; set; }

    // CompletionProofPicture stores base64 encoded image - no length validation needed
    public string? CompletionProofPicture { get; set; }

    [Required(ErrorMessage = "Completion date is required.")]
    public DateTime CompletionDate { get; set; }

    [StringLength(RescueCompletionConsts.MaxLength.CompletionDescription, ErrorMessage = "Completion description must not exceed {1} characters.")]
    public string? CompletionDescription { get; set; }
}