using Pawchums.Entities.RequestRescues;
using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace AnimalRescueSystem.Entities.RequestRescues;

/// <summary>
/// Tracks notifications sent to rescuers about new rescue requests
/// </summary>
public class RescuerNotification : CreationAuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    // Foreign Keys
    public virtual Guid RequestRescueId { get; set; }
    public virtual Guid RescuerId { get; set; } // Foreign key to AbpUsers.Id

    // Notification Details
    public virtual DateTime SentDate { get; set; }
    public virtual bool IsRead { get; set; }
    public virtual DateTime? ReadDate { get; set; }
    public virtual string NotificationType { get; set; } // NewRequest, RequestUpdated, RequestCancelled
    public virtual string? Message { get; set; }

    // Navigation Property
    public virtual RequestRescue RequestRescue { get; set; }
}