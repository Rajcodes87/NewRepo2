using AnimalRescueSystem.Constants;
using System.ComponentModel.DataAnnotations;

namespace AnimalRescueSystem.RescueCompletions;

public class VerifyCompletionDto
{
    [StringLength(RescueCompletionConsts.MaxLength.VerificationNotes, ErrorMessage = "Verification notes must not exceed {1} characters.")]
    public string? VerificationNotes { get; set; }
}