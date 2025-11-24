using System.ComponentModel.DataAnnotations;

namespace AnimalRescueSystem.RequestRescues;

public class CreateUpdateRequestRescueDto
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; }

    [Required]
    [StringLength(500)]
    public string Location { get; set; }

    [Required]
    [StringLength(2000)]
    public string Description { get; set; }

    public string? Picture { get; set; }

    [Required]
    [StringLength(20)]
    public string ContactNo { get; set; }

    [StringLength(100)]
    public string? ContactName { get; set; }

    [Required]
    [StringLength(20)]
    public string Severity { get; set; }

    // ✅ NEW: GPS Coordinates
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? MapUrl { get; set; }
}