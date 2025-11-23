using System;

namespace AnimalRescueSystem.RequestRescues;

public class RescuerNotificationDto
{
    public Guid Id { get; set; }
    public Guid RequestRescueId { get; set; }
    public string RequestTitle { get; set; }
    public string RequestLocation { get; set; }
    public DateTime SentDate { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadDate { get; set; }
    public string NotificationType { get; set; }
    public string? Message { get; set; }
}