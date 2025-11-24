namespace AnimalRescueSystem.RequestRescues;

public class RequestRescueFilter
{
    public string? SearchKeyword { get; set; }
    public string? Status { get; set; }
    public string? Severity { get; set; }
    public bool? IsActive { get; set; }
}