using System;
using Volo.Abp.Application.Dtos;

namespace AnimalRescueSystem.RescuerProfiles;

public class RescuerProfileDto : FullAuditedEntityDto<Guid>
{
    public Guid? TenantId { get; set; }
    public Guid UserId { get; set; }

    // Location Information
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? CurrentLocation { get; set; }
    public DateTime? LocationUpdatedAt { get; set; }
    public bool IsLocationShared { get; set; }

    // Notification Preferences
    public int MaxNotificationRadius { get; set; }
    public bool ReceiveEmailNotifications { get; set; }
    public bool ReceiveSmsNotifications { get; set; }
}