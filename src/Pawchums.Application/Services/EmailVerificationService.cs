using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pawchums.Entities.EmailVerification;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Emailing;
using Volo.Abp.TextTemplating;

namespace Pawchums.Services;

public class EmailVerificationService : ITransientDependency
{
    private readonly IRepository<EmailVerificationCode, Guid> _verificationCodeRepository;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<EmailVerificationService> _logger;

    public EmailVerificationService(
        IRepository<EmailVerificationCode, Guid> verificationCodeRepository,
        IEmailSender emailSender,
        ILogger<EmailVerificationService> logger)
    {
        _verificationCodeRepository = verificationCodeRepository;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task<string> GenerateAndSendVerificationCodeAsync(
        string email,
        string purpose,
        Guid? userId = null)
    {
        // Invalidate any existing codes for this email and purpose
        var existingCodes = await _verificationCodeRepository.GetListAsync(
            x => x.Email == email && x.Purpose == purpose && !x.IsUsed);

        foreach (var code in existingCodes)
        {
            code.IsUsed = true;
            await _verificationCodeRepository.UpdateAsync(code);
        }

        // Generate new 6-digit code
        var verificationCode = GenerateRandomCode();

        // Store the verification code
        var emailVerification = new EmailVerificationCode(
            Guid.NewGuid(),
            email,
            verificationCode,
            purpose,
            userId,
            expirationMinutes: 15 // Code expires in 15 minutes
        );

        await _verificationCodeRepository.InsertAsync(emailVerification);

        // Send email
        await SendVerificationEmailAsync(email, verificationCode, purpose);

        _logger.LogInformation($"Verification code sent to {email} for {purpose}");

        return verificationCode;
    }

    public async Task<bool> VerifyCodeAsync(string email, string code, string purpose)
    {
        var verificationCode = await _verificationCodeRepository.FirstOrDefaultAsync(
            x => x.Email == email &&
                 x.Code == code &&
                 x.Purpose == purpose &&
                 !x.IsUsed);

        if (verificationCode == null)
        {
            _logger.LogWarning($"Verification code not found for {email}");
            return false;
        }

        if (verificationCode.IsExpired())
        {
            _logger.LogWarning($"Verification code expired for {email}");
            return false;
        }

        // Mark as used
        verificationCode.IsUsed = true;
        await _verificationCodeRepository.UpdateAsync(verificationCode);

        _logger.LogInformation($"Verification code validated successfully for {email}");
        return true;
    }

    private string GenerateRandomCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }

    private async Task SendVerificationEmailAsync(string email, string code, string purpose)
    {
        string subject;
        string body;

        if (purpose == "Registration")
        {
            subject = "Verify Your Email - Pawchums Animal Rescue";
            body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px;'>
                        <h2 style='color: #52c41a; text-align: center;'>Welcome to Pawchums!</h2>
                        <p>Thank you for registering as a Rescuer with Pawchums Animal Rescue System.</p>
                        <p>To complete your registration and activate your account, please use the following verification code:</p>
                        <div style='text-align: center; margin: 30px 0;'>
                            <span style='font-size: 32px; font-weight: bold; letter-spacing: 5px; color: #52c41a; background: #f0f0f0; padding: 15px 30px; border-radius: 5px;'>
                                {code}
                            </span>
                        </div>
                        <p>This code will expire in <strong>15 minutes</strong>.</p>
                        <p>If you didn't request this registration, please ignore this email.</p>
                        <hr style='margin: 30px 0; border: none; border-top: 1px solid #ddd;'>
                        <p style='font-size: 12px; color: #888; text-align: center;'>
                            Pawchums Animal Rescue System - Saving Lives, One Paw at a Time
                        </p>
                    </div>
                </body>
                </html>
            ";
        }
        else if (purpose == "PasswordReset")
        {
            subject = "Password Reset Code - Pawchums Animal Rescue";
            body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px;'>
                        <h2 style='color: #ff4d4f; text-align: center;'>Password Reset Request</h2>
                        <p>We received a request to reset your password for your Pawchums account.</p>
                        <p>To reset your password, please use the following verification code:</p>
                        <div style='text-align: center; margin: 30px 0;'>
                            <span style='font-size: 32px; font-weight: bold; letter-spacing: 5px; color: #ff4d4f; background: #f0f0f0; padding: 15px 30px; border-radius: 5px;'>
                                {code}
                            </span>
                        </div>
                        <p>This code will expire in <strong>15 minutes</strong>.</p>
                        <p>If you didn't request a password reset, please ignore this email and your password will remain unchanged.</p>
                        <hr style='margin: 30px 0; border: none; border-top: 1px solid #ddd;'>
                        <p style='font-size: 12px; color: #888; text-align: center;'>
                            Pawchums Animal Rescue System - Saving Lives, One Paw at a Time
                        </p>
                    </div>
                </body>
                </html>
            ";
        }
        else
        {
            throw new UserFriendlyException($"Invalid purpose: {purpose}");
        }

        try
        {
            await _emailSender.SendAsync(email, subject, body);
            _logger.LogInformation($"Email sent successfully to {email}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send email to {email}");
            throw new UserFriendlyException("Failed to send verification email. Please try again later.");
        }
    }
}
