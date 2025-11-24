using System;
using System.ComponentModel.DataAnnotations;

namespace AnimalRescueSystem.RescuerProfiles;

public class ShareLocationDto
{
    [Required]
    public string Token { get; set; }

    [Required]
    public Guid RequestRescueId { get; set; }

    [Required]
    public double Latitude { get; set; }

    [Required]
    public double Longitude { get; set; }

    public string? CurrentLocation { get; set; }
}