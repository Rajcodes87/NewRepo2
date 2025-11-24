using AnimalRescueSystem.Constants;
using Microsoft.Extensions.Logging;
using Pawchums.Entities.RescuerApplication;
using Pawchums.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Emailing;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;

namespace AnimalRescueSystem.Services;

public class RescuerApplicationEmailService : ApplicationService
{
    private readonly IEmailSender _emailSender;
    private readonly IIdentityUserRepository _userRepository;
    private readonly IIdentityRoleRepository _roleRepository;
    private readonly ILogger<RescuerApplicationEmailService> _logger;

    public RescuerApplicationEmailService(
        IEmailSender emailSender,
        IIdentityUserRepository userRepository,
        IIdentityRoleRepository roleRepository,
        ILogger<RescuerApplicationEmailService> logger)
    {
        _emailSender = emailSender;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _logger = logger;
    }

    /// <summary>
    /// Notify all admins of a new rescuer application
    /// </summary>
    public async Task NotifyAdminsOfNewApplicationAsync(RescuerApplication application)
    {
        try
        {
            _logger.LogInformation("RescuerApplicationEmailService - NotifyAdminsOfNewApplicationAsync: Sending notification for application {ApplicationId}", application.Id);

            // Get all admin users
            var adminRole = await _roleRepository.FindByNormalizedNameAsync("ADMIN");
            if (adminRole == null)
            {
                _logger.LogWarning("RescuerApplicationEmailService - NotifyAdminsOfNewApplicationAsync: Admin role not found");
                return;
            }

            var adminUsers = await _userRepository.GetListByNormalizedRoleNameAsync(adminRole.NormalizedName);

            if (!adminUsers.Any())
            {
                _logger.LogWarning("RescuerApplicationEmailService - NotifyAdminsOfNewApplicationAsync: No admin users found");
                return;
            }

            // Prepare email body
            var emailBody = GetAdminNotificationEmailBody(application);

            // Send email to each admin
            foreach (var admin in adminUsers)
            {
                if (!string.IsNullOrEmpty(admin.Email))
                {
                    await _emailSender.SendAsync(
                        to: admin.Email,
                        subject: $"New Rescuer Application Pending Review - {application.Name} {application.Surname}",
                        body: emailBody,
                        isBodyHtml: true);

                    _logger.LogInformation("RescuerApplicationEmailService - NotifyAdminsOfNewApplicationAsync: Email sent to admin {Email}", admin.Email);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RescuerApplicationEmailService - NotifyAdminsOfNewApplicationAsync: Error sending notifications");
            // Don't throw - email failure shouldn't block the application creation
        }
    }

    /// <summary>
    /// Notify applicant of application review result
    /// </summary>
    public async Task NotifyApplicantOfReviewAsync(RescuerApplication application)
    {
        try
        {
            _logger.LogInformation("RescuerApplicationEmailService - NotifyApplicantOfReviewAsync: Sending notification for application {ApplicationId}", application.Id);

            if (string.IsNullOrEmpty(application.Email))
            {
                _logger.LogWarning("RescuerApplicationEmailService - NotifyApplicantOfReviewAsync: No email address for applicant");
                return;
            }

            var emailBody = GetApplicantNotificationEmailBody(application);
            var subject = application.Status == RescuerApplicationConsts.ApplicationStatus.Approved
                ? "Your Rescuer Application Has Been Approved!"
                : "Your Rescuer Application Has Been Rejected";

            await _emailSender.SendAsync(
                to: application.Email,
                subject: subject,
                body: emailBody,
                isBodyHtml: true);

            _logger.LogInformation("RescuerApplicationEmailService - NotifyApplicantOfReviewAsync: Email sent to applicant {Email}", application.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RescuerApplicationEmailService - NotifyApplicantOfReviewAsync: Error sending notification for application {ApplicationId}", application.Id);
            // Don't throw - email failure shouldn't block the review
        }
    }

    private string GetAdminNotificationEmailBody(RescuerApplication application)
    {
        return $@"
<html>
<body style='font-family: Arial, sans-serif;'>
    <h2>New Rescuer Application Pending Review</h2>
    
    <p>A new rescuer application has been submitted and requires your review.</p>
    
    <h3>Applicant Information:</h3>
    <table style='border-collapse: collapse; width: 100%;'>
        <tr>
            <td style='border: 1px solid #ddd; padding: 8px;'><strong>Name:</strong></td>
            <td style='border: 1px solid #ddd; padding: 8px;'>{application.Name} {application.Surname}</td>
        </tr>
        <tr>
            <td style='border: 1px solid #ddd; padding: 8px;'><strong>Email:</strong></td>
            <td style='border: 1px solid #ddd; padding: 8px;'>{application.Email}</td>
        </tr>
        <tr>
            <td style='border: 1px solid #ddd; padding: 8px;'><strong>Username:</strong></td>
            <td style='border: 1px solid #ddd; padding: 8px;'>{application.UserName}</td>
        </tr>
        <tr>
            <td style='border: 1px solid #ddd; padding: 8px;'><strong>Phone:</strong></td>
            <td style='border: 1px solid #ddd; padding: 8px;'>{application.PhoneNumber}</td>
        </tr>
        <tr>
            <td style='border: 1px solid #ddd; padding: 8px;'><strong>Application Date:</strong></td>
            <td style='border: 1px solid #ddd; padding: 8px;'>{application.ApplicationDate:yyyy-MM-dd HH:mm:ss}</td>
        </tr>
    </table>
    
    <h3>Identity Card Photo:</h3>
    <p>Please log in to the admin panel to view the identity card photo and verify the applicant's identity.</p>
    
    <p style='color: #666; margin-top: 20px; font-size: 12px;'>
        Please review this application and update the status as needed.
    </p>
</body>
</html>";
    }

    private string GetApplicantNotificationEmailBody(RescuerApplication application)
    {
        var statusMessage = application.Status == RescuerApplicationConsts.ApplicationStatus.Approved
            ? "Your identity verification has been approved! You are now officially a verified Rescuer in our system. You can now participate in rescue operations and help animals in need."
            : $"Unfortunately, your identity verification was not approved at this time. Reason: {application.ReviewNotes ?? "Please contact support for more information."} You may reapply after addressing the concerns.";

        return $@"
<html>
<body style='font-family: Arial, sans-serif;'>
    <h2>Rescuer Application Review Result</h2>
    
    <p>Dear {application.Name},</p>
    
    <p>Your rescuer application has been reviewed by our admin team.</p>
    
    <h3>Application Status: <span style='color: {(application.Status == RescuerApplicationConsts.ApplicationStatus.Approved ? "green" : "red")};'>{application.Status}</span></h3>
    
    <p>{statusMessage}</p>
    
    {(application.ReviewNotes != null ? $"<h4>Review Notes:</h4><p>{application.ReviewNotes}</p>" : "")}
    
    <p>If you have any questions, please contact our support team.</p>
    
    <p style='color: #666; margin-top: 20px; font-size: 12px;'>
        Thank you for joining our rescue community!
    </p>
</body>
</html>";
    }
}