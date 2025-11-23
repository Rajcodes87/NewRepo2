using System;

namespace AnimalRescueSystem.RescueInitiations;

public class RescueInitiationDto
{
    public Guid Id { get; set; }
    public Guid RequestRescueId { get; set; }
    public string RequestTitle { get; set; }
    public string RequestLocation { get; set; }
    public Guid RescuerId { get; set; }
    public string? RescuerName { get; set; }
    public string? RescuerEmail { get; set; }
    public DateTime InitiatedDate { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; }
    public bool IsSelected { get; set; }
    public DateTime? AcceptedDate { get; set; }
    public Guid? AcceptedByUserId { get; set; }
    public string? AcceptedByUserName { get; set; }
    public DateTime CreationTime { get; set; }
}