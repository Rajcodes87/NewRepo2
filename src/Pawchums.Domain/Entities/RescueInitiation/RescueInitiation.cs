using Pawchums.Entities.RequestRescues;
using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace AnimalRescueSystem.Entities.RequestRescues;

/// <summary>
/// Tracks when rescuers initiate/accept a rescue request
/// Multiple rescuers can express interest, but only one is selected
/// </summary>
public class RescueInitiation : FullAuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    // Foreign Keys
    public virtual Guid RequestRescueId { get; set; }
    public virtual Guid RescuerId { get; set; } // Foreign key to AbpUsers.Id

    // Initiation Details
    public virtual DateTime InitiatedDate { get; set; }
    public virtual string? Notes { get; set; } // Rescuer's notes when initiating
    public virtual string Status { get; set; } // Pending, Accepted, Rejected, Withdrawn
    public virtual bool IsSelected { get; set; } // True if this rescuer was selected for the job
    public virtual DateTime? AcceptedDate { get; set; } // When admin accepted this rescuer
    public virtual Guid? AcceptedByUserId { get; set; } // Admin who accepted

    // Navigation Properties
    public virtual RequestRescue RequestRescue { get; set; }
}