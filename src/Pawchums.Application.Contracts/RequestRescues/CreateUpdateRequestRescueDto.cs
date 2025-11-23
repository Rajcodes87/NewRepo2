using AnimalRescueSystem.Constants;
using System;
using System.ComponentModel.DataAnnotations;

namespace AnimalRescueSystem.RequestRescues;

public class CreateUpdateRequestRescueDto
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(RequestRescueConsts.MaxLength.Title, ErrorMessage = "Title must not exceed {1} characters.")]
    public string Title { get; set; }

    [Required(ErrorMessage = "Location is required.")]
    [StringLength(RequestRescueConsts.MaxLength.Location, ErrorMessage = "Location must not exceed {1} characters.")]
    public string Location { get; set; }

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(RequestRescueConsts.MaxLength.Description, ErrorMessage = "Description must not exceed {1} characters.")]
    public string Description { get; set; }

    // Picture field stores base64 encoded image - no length validation needed
    public string? Picture { get; set; }

    [Required(ErrorMessage = "Contact number is required.")]
    [StringLength(RequestRescueConsts.MaxLength.ContactNo, ErrorMessage = "Contact number must not exceed {1} characters.")]
    public string ContactNo { get; set; }

    [StringLength(RequestRescueConsts.MaxLength.ContactName, ErrorMessage = "Contact name must not exceed {1} characters.")]
    public string? ContactName { get; set; }

    public bool IsActive { get; set; } = true;
}