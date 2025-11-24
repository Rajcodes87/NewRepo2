using System;

namespace AnimalRescueSystem.RescuerProfiles;

public class RescuerDistanceDto
{
    public Guid RescuerId { get; set; }
    public string RescuerName { get; set; }
    public string Email { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double Distance { get; set; } // in kilometers
    public string DistanceDisplay => $"{Distance:F1} km";
}