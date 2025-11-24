using System.ComponentModel.DataAnnotations;

namespace AnimalRescueSystem.RescuerProfiles;

public class CreateUpdateRescuerProfileDto
{
    // Location Information
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    [StringLength(500)]
    public string? CurrentLocation { get; set; }

    public bool IsLocationShared { get; set; }

    // Notification Preferences
    [Range(1, 200)]
    public int MaxNotificationRadius { get; set; } = 50;

    public bool ReceiveEmailNotifications { get; set; } = true;
    public bool ReceiveSmsNotifications { get; set; } = false;
}