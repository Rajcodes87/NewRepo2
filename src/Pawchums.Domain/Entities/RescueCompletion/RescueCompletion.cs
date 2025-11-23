using Pawchums.Entities.RequestRescues;
using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace AnimalRescueSystem.Entities.RequestRescues;

/// <summary>
/// Tracks the completion/proof of rescue for a request
/// One-to-one relationship with RequestRescue
/// </summary>
public class RescueCompletion : FullAuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    // Foreign Key
    public virtual Guid RequestRescueId { get; set; }

    // Completion Details
    public virtual string? CompletionProofPicture { get; set; }
    public virtual DateTime CompletionDate { get; set; }
    public virtual string? CompletionDescription { get; set; }
    public virtual Guid CompletedByRescuerId { get; set; } // Who completed it

    // Verification (optional for future use)
    public virtual bool IsVerified { get; set; }
    public virtual Guid? VerifiedByUserId { get; set; }
    public virtual DateTime? VerifiedDate { get; set; }
    public virtual string? VerificationNotes { get; set; }

    // Navigation Property
    public virtual RequestRescue RequestRescue { get; set; }
}