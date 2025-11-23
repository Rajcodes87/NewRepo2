using System;

namespace AnimalRescueSystem.RescueCompletions;

public class RescueCompletionFilter
{
    public string? SearchKeyword { get; set; }
    public bool? IsVerified { get; set; }
    public Guid? RequestRescueId { get; set; }
    public Guid? CompletedByRescuerId { get; set; }
    public DateTime? CompletionDateFrom { get; set; }
    public DateTime? CompletionDateTo { get; set; }
}