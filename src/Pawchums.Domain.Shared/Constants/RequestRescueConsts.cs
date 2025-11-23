namespace AnimalRescueSystem.Constants;

public static class RequestRescueConsts
{
    public static class MaxLength
    {
        public const int Title = 256;
        public const int Location = 512;
        public const int Description = 2000;
        // Picture will store base64 encoded image - using -1 to indicate unlimited/max for validation
        // Actual DB column will be configured as TEXT/NVARCHAR(MAX)
        public const int Picture = -1;
        public const int ContactNo = 20;
        public const int ContactName = 100;
        public const int Status = 50;
    }

    public static class RequestStatus
    {
        public const string NotInitiated = "NotInitiated"; // No rescuer has shown interest yet
        public const string Initiated = "Initiated"; // At least one rescuer has shown interest
        public const string InProgress = "InProgress"; // A rescuer has been selected and accepted
        public const string Completed = "Completed";
        public const string Cancelled = "Cancelled";
    }
}

public static class RescueInitiationConsts
{
    public static class MaxLength
    {
        public const int Notes = 1000;
        public const int Status = 50;
    }

    public static class InitiationStatus
    {
        public const string Pending = "Pending"; // Waiting for admin approval
        public const string Accepted = "Accepted"; // Admin accepted this rescuer
        public const string Rejected = "Rejected"; // Admin rejected
        public const string Withdrawn = "Withdrawn"; // Rescuer withdrew their interest
    }
}

public static class RescueCompletionConsts
{
    public static class MaxLength
    {
        // CompletionProofPicture will store base64 encoded image - using -1 to indicate unlimited/max for validation
        public const int CompletionProofPicture = -1;
        public const int CompletionDescription = 2000;
        public const int VerificationNotes = 1000;
    }
}

public static class RescuerNotificationConsts
{
    public static class MaxLength
    {
        public const int NotificationType = 50;
        public const int Message = 1000;
    }

    public static class NotificationType
    {
        public const string NewRequest = "NewRequest";
        public const string RequestUpdated = "RequestUpdated";
        public const string RequestCancelled = "RequestCancelled";
        public const string InitiationAccepted = "InitiationAccepted";
        public const string InitiationRejected = "InitiationRejected";
    }
}