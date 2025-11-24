using System;

namespace AnimalRescueSystem.RescuerProfiles;

public class RescuerLocationDto
{
    public Guid RescuerId { get; set; }
    public string RescuerName { get; set; }
    public string Email { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}