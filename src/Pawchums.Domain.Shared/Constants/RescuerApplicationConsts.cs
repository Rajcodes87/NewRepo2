namespace AnimalRescueSystem.Constants;

public static class RescuerApplicationConsts
{
    public static class MaxLength
    {
        public const int UserName = 256;
        public const int Email = 256;
        public const int Name = 64;
        public const int Surname = 64;
        public const int PhoneNumber = 16;
        // IdentityCardPicture stores base64 image - using -1 to indicate unlimited/max for validation
        public const int IdentityCardPicture = -1;
        public const int Status = 50;
        public const int ReviewNotes = 1000;
    }

    public static class ApplicationStatus
    {
        public const string Pending = "Pending";
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";
    }
}
