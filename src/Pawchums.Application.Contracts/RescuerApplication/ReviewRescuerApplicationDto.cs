using System.ComponentModel.DataAnnotations;

namespace AnimalRescueSystem.RescuerApplications;

public class ReviewRescuerApplicationDto
{
    [Required(ErrorMessage = "Application ID is required.")]
    public string ApplicationId { get; set; }

    [Required(ErrorMessage = "Status is required.")]
    [RegularExpression("^(Approved|Rejected)$", ErrorMessage = "Status must be either 'Approved' or 'Rejected'.")]
    public string Status { get; set; }

    [StringLength(1000, ErrorMessage = "Review notes cannot exceed 1000 characters.")]
    public string? ReviewNotes { get; set; }
}