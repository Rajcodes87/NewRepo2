using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace AnimalRescueSystem.Entities.Base;

public abstract class DescriptiveEntityBase : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public virtual string SystemName { get; set; }
    public virtual string DisplayName { get; set; }
    public virtual string? Description { get; set; }
    public virtual bool IsActive { get; set; }
}
