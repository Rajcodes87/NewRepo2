using System;

namespace AnimalRescueSystem.RescuerApplications;

public class RescuerApplicationDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string PhoneNumber { get; set; }
    public string? IdentityCardPicture { get; set; }
    public string Status { get; set; }
    public DateTime ApplicationDate { get; set; }
    public Guid? ReviewedByUserId { get; set; }
    public DateTime? ReviewedDate { get; set; }
    public string? ReviewNotes { get; set; }
}