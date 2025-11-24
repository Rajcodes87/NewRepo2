using AnimalRescueSystem.Entities.RequestRescues;
using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Pawchums.Entities.RequestRescues;

public class RequestRescue : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    // Request Information (Posted by anyone - can be anonymous)
    public virtual string Title { get; set; }
    public virtual string Location { get; set; }
    public virtual string Description { get; set; }
    public virtual string? Picture { get; set; }
    public virtual string ContactNo { get; set; }
    public virtual string? ContactName { get; set; }
    public virtual DateTime RequestDate { get; set; }

    // Status Management
    public virtual string Status { get; set; } // NotInitiated, Initiated, InProgress, Completed, Cancelled

    // Severity
    public virtual string Severity { get; set; } // Low, Medium, High

    // Active Status
    public virtual bool IsActive { get; set; }
    public virtual double? Latitude { get; set; }
    public virtual double? Longitude { get; set; }
    public virtual string? MapUrl { get; set; }
    // Navigation Properties
    public virtual ICollection<RescueInitiation> RescueInitiations { get; set; }
    public virtual RescueCompletion? RescueCompletion { get; set; }

    public RequestRescue()
    {
        RescueInitiations = new List<RescueInitiation>();
    }
}