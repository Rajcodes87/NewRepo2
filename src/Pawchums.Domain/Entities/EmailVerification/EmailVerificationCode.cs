using System;
using Volo.Abp.Domain.Entities;

namespace Pawchums.Entities.EmailVerification;

public class EmailVerificationCode : Entity<Guid>
{
    public string Email { get; set; }
    public string Code { get; set; }
    public string Purpose { get; set; } // "Registration" or "PasswordReset"
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public Guid? UserId { get; set; }

    protected EmailVerificationCode()
    {
    }

    public EmailVerificationCode(
        Guid id,
        string email,
        string code,
        string purpose,
        Guid? userId = null,
        int expirationMinutes = 15)
        : base(id)
    {
        Email = email;
        Code = code;
        Purpose = purpose;
        UserId = userId;
        CreatedAt = DateTime.UtcNow;
        ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);
        IsUsed = false;
    }

    public bool IsExpired()
    {
        return DateTime.UtcNow > ExpiresAt;
    }

    public bool IsValid()
    {
        return !IsUsed && !IsExpired();
    }
}
