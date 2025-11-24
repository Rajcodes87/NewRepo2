using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Pawchums.Entities.RescuerProfile;

/// <summary>
/// Stores rescuer's location preferences and current location
/// </summary>
public class RescuerProfile : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    // User Reference
    public virtual Guid UserId { get; set; }

    // Location Information
    public virtual double? Latitude { get; set; }
    public virtual double? Longitude { get; set; }
    public virtual string? CurrentLocation { get; set; }
    public virtual DateTime? LocationUpdatedAt { get; set; }
    public virtual bool IsLocationShared { get; set; }

    // Notification Preferences
    public virtual int MaxNotificationRadius { get; set; } // in kilometers
    public virtual bool ReceiveEmailNotifications { get; set; }
    public virtual bool ReceiveSmsNotifications { get; set; }

    public RescuerProfile()
    {
        IsLocationShared = false;
        MaxNotificationRadius = 50; // Default 50km radius
        ReceiveEmailNotifications = true;
        ReceiveSmsNotifications = false;
    }
}