using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;

namespace Pawchums.Entities.RescuerApplication;

/// <summary>
/// Tracks rescuer applications for approval
/// </summary>
public class RescuerApplication : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    // User Information
    public virtual Guid UserId { get; set; }
    public virtual string UserName { get; set; }
    public virtual string Email { get; set; }
    public virtual string Name { get; set; }
    public virtual string Surname { get; set; }
    public virtual string PhoneNumber { get; set; }

    // Application Details
    public virtual string? IdentityCardPicture { get; set; } // Base64 encoded image
    public virtual string Status { get; set; } // Pending, Approved, Rejected
    public virtual DateTime ApplicationDate { get; set; }

    // Review Information
    public virtual Guid? ReviewedByUserId { get; set; }
    public virtual DateTime? ReviewedDate { get; set; }
    public virtual string? ReviewNotes { get; set; }

    public RescuerApplication()
    {
    }
}
