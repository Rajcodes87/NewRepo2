using AnimalRescueSystem.Constants;
using System;
using System.ComponentModel.DataAnnotations;

namespace AnimalRescueSystem.RescueCompletions;

public class UpdateRescueCompletionDto
{
    // CompletionProofPicture stores base64 encoded image - no length validation needed
    public string? CompletionProofPicture { get; set; }

    [Required(ErrorMessage = "Completion date is required.")]
    public DateTime CompletionDate { get; set; }

    [StringLength(RescueCompletionConsts.MaxLength.CompletionDescription, ErrorMessage = "Completion description must not exceed {1} characters.")]
    public string? CompletionDescription { get; set; }
}