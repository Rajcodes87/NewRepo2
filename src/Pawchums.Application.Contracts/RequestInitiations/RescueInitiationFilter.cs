using System;

namespace AnimalRescueSystem.RescueInitiations;

public class RescueInitiationFilter
{
    public string? SearchKeyword { get; set; }
    public string? Status { get; set; }
    public bool? IsSelected { get; set; }
    public Guid? RequestRescueId { get; set; }
    public Guid? RescuerId { get; set; }
}