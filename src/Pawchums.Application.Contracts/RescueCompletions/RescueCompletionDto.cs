using System;

namespace Pawchums.RescueCompletions;

public class RescueCompletionDto
{
    public Guid Id { get; set; }
    public Guid RequestRescueId { get; set; }
    public string RequestTitle { get; set; }
    public string RequestLocation { get; set; }
    public string? CompletionProofPicture { get; set; }
    public DateTime CompletionDate { get; set; }
    public string? CompletionDescription { get; set; }
    public Guid CompletedByRescuerId { get; set; }
    public string? CompletedByRescuerName { get; set; }
    public string? CompletedByRescuerEmail { get; set; }
    public bool IsVerified { get; set; }
    public Guid? VerifiedByUserId { get; set; }
    public string? VerifiedByUserName { get; set; }
    public DateTime? VerifiedDate { get; set; }
    public string? VerificationNotes { get; set; }
    public DateTime CreationTime { get; set; }
}