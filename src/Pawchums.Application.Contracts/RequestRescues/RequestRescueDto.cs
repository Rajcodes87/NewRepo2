using System;

namespace AnimalRescueSystem.RequestRescues;

public class RequestRescueDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Location { get; set; }
    public string Description { get; set; }
    public string? Picture { get; set; }
    public string ContactNo { get; set; }
    public string? ContactName { get; set; }
    public DateTime RequestDate { get; set; }
    public string Status { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreationTime { get; set; }

    // Related data counts
    public int InitiationsCount { get; set; }
    public bool HasCompletion { get; set; }
}