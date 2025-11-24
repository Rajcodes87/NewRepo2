using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AnimalRescueSystem.RescuerProfiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Pawchums.Entities.RequestRescues;
using Volo.Abp.Application.Services;
using Volo.Abp.Emailing;
using Volo.Abp.Identity;

namespace Pawchums.Services;

/// <summary>
/// Service for sending email notifications to rescuers about new rescue requests
/// </summary>
public class RescuerNotificationEmailService : ApplicationService
{
    private readonly IEmailSender _emailSender;
    private readonly IIdentityUserRepository _userRepository;
    private readonly IIdentityRoleRepository _roleRepository;
    private readonly DistanceCalculationService _distanceService;
    private readonly LocationTokenService _tokenService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RescuerNotificationEmailService> _logger;

    public RescuerNotificationEmailService(
        IEmailSender emailSender,
        IIdentityUserRepository userRepository,
        IIdentityRoleRepository roleRepository,
        DistanceCalculationService distanceService,
        LocationTokenService tokenService,
        IConfiguration configuration,
        ILogger<RescuerNotificationEmailService> logger)
    {
        _emailSender = emailSender;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _distanceService = distanceService;
        _tokenService = tokenService;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Notify rescuers about a new rescue request with distance calculations
    /// </summary>
    public async Task NotifyRescuersAboutNewRequestAsync(
        RequestRescue request,
        List<RescuerDistanceDto> nearestRescuers)
    {
        try
        {
            _logger.LogInformation("Sending notifications to {Count} rescuers for request {RequestId}",
                nearestRescuers.Count, request.Id);

            var frontendUrl = _configuration["App:FrontendUrl"] ?? "http://localhost:3000";

            foreach (var rescuer in nearestRescuers)
            {
                try
                {
                    // Generate unique token for this rescuer and request
                    var token = await _tokenService.GenerateTokenAsync(request.Id, rescuer.RescuerId);

                    // Generate email body
                    var emailBody = await GetRescueNotificationEmailBodyAsync(request, rescuer, token, frontendUrl);

                    // Send email
                    await _emailSender.SendAsync(
                        to: rescuer.Email,
                        subject: $"🚨 Urgent: Animal Rescue Needed - {rescuer.DistanceDisplay} Away",
                        body: emailBody,
                        isBodyHtml: true);

                    _logger.LogInformation("Notification sent to rescuer {RescuerId} ({Email})", 
                        rescuer.RescuerId, rescuer.Email);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send notification to rescuer {RescuerId}", rescuer.RescuerId);
                    // Continue sending to other rescuers even if one fails
                }
            }

            _logger.LogInformation("Completed sending notifications for request {RequestId}", request.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending rescuer notifications for request {RequestId}", request.Id);
            throw;
        }
    }

    /// <summary>
    /// Generate HTML email body for rescue notification
    /// </summary>
    private async Task<string> GetRescueNotificationEmailBodyAsync(
        RequestRescue request,
        RescuerDistanceDto rescuer,
        string locationToken,
        string frontendUrl)
    {
        var locationShareUrl = $"{frontendUrl}/share-location?token={locationToken}&requestId={request.Id}";
        
        // OpenStreetMap URL for viewing rescue location
        var osmMapUrl = request.Latitude.HasValue && request.Longitude.HasValue
            ? $"https://www.openstreetmap.org/?mlat={request.Latitude}&mlon={request.Longitude}#map=15/{request.Latitude}/{request.Longitude}"
            : "#";

        // Google Maps URL (alternative)
        var googleMapsUrl = request.Latitude.HasValue && request.Longitude.HasValue
            ? $"https://www.google.com/maps?q={request.Latitude},{request.Longitude}"
            : "#";

        var severityColor = request.Severity switch
        {
            "Critical" => "#ff4d4f",
            "High" => "#ff7a45",
            "Medium" => "#ffa940",
            "Low" => "#52c41a",
            _ => "#1890ff"
        };

        return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>New Rescue Request</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; background-color: #f5f5f5;'>
    <div style='max-width: 600px; margin: 20px auto; background: white; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 8px rgba(0,0,0,0.1);'>
        
        <!-- Header -->
        <div style='background: linear-gradient(135deg, #52c41a 0%, #73d13d 100%); color: white; padding: 30px 20px; text-align: center;'>
            <h1 style='margin: 0; font-size: 28px;'>🚨 New Rescue Request</h1>
            <p style='margin: 10px 0 0 0; font-size: 16px; opacity: 0.9;'>An animal needs your help nearby!</p>
        </div>

        <!-- Distance Badge -->
        <div style='background: #fff7e6; border-left: 4px solid #ffa940; padding: 15px 20px; margin: 20px;'>
            <div style='display: flex; align-items: center; justify-content: space-between;'>
                <div>
                    <p style='margin: 0; font-size: 14px; color: #666;'>Distance from you:</p>
                    <h2 style='margin: 5px 0 0 0; color: #fa8c16; font-size: 32px;'>{rescuer.DistanceDisplay}</h2>
                </div>
                <div style='font-size: 48px;'>📍</div>
            </div>
        </div>

        <!-- Request Details -->
        <div style='padding: 0 20px;'>
            <div style='background: #f9f9f9; padding: 20px; border-radius: 8px; margin-bottom: 20px;'>
                <h2 style='margin: 0 0 15px 0; color: #333; font-size: 22px;'>{request.Title}</h2>
                
                <div style='margin-bottom: 12px;'>
                    <span style='display: inline-block; background: {severityColor}; color: white; padding: 4px 12px; border-radius: 12px; font-size: 12px; font-weight: bold;'>
                        {request.Severity.ToUpper()} PRIORITY
                    </span>
                </div>

                <div style='margin-bottom: 10px;'>
                    <strong>📍 Location:</strong>
                    <p style='margin: 5px 0 0 0; color: #666;'>{request.Location}</p>
                </div>

                <div style='margin-bottom: 10px;'>
                    <strong>📝 Description:</strong>
                    <p style='margin: 5px 0 0 0; color: #666;'>{request.Description}</p>
                </div>

                <div style='margin-bottom: 10px;'>
                    <strong>📞 Contact:</strong>
                    <p style='margin: 5px 0 0 0; color: #666;'>
                        {request.ContactNo}
                        {(!string.IsNullOrEmpty(request.ContactName) ? $" ({request.ContactName})" : "")}
                    </p>
                </div>

                <div>
                    <strong>🕒 Reported:</strong>
                    <p style='margin: 5px 0 0 0; color: #666;'>{request.RequestDate:MMM dd, yyyy HH:mm}</p>
                </div>
            </div>

            <!-- Action Buttons -->
            <div style='margin-bottom: 30px;'>
                <h3 style='color: #333; font-size: 18px; margin-bottom: 15px;'>Take Action:</h3>
                
                <!-- Share Location Button -->
                <a href='{locationShareUrl}' 
                   style='display: block; background: #52c41a; color: white; text-align: center; padding: 15px 20px; text-decoration: none; border-radius: 6px; font-weight: bold; font-size: 16px; margin-bottom: 10px;'>
                    📲 Share Your Current Location
                </a>

                <!-- View on Map Buttons -->
                <div style='display: grid; grid-template-columns: 1fr 1fr; gap: 10px; margin-bottom: 10px;'>
                    <a href='{osmMapUrl}' 
                       style='display: block; background: #1890ff; color: white; text-align: center; padding: 12px; text-decoration: none; border-radius: 6px; font-size: 14px;'>
                        🗺️ OpenStreetMap
                    </a>
                    <a href='{googleMapsUrl}' 
                       style='display: block; background: #1890ff; color: white; text-align: center; padding: 12px; text-decoration: none; border-radius: 6px; font-size: 14px;'>
                        🗺️ Google Maps
                    </a>
                </div>

                <!-- View in App Button -->
                <a href='{frontendUrl}/rescue-requests/{request.Id}' 
                   style='display: block; background: white; color: #1890ff; text-align: center; padding: 12px; text-decoration: none; border-radius: 6px; border: 2px solid #1890ff; font-weight: 500;'>
                    👀 View Details in App
                </a>
            </div>

            <!-- Info Box -->
            <div style='background: #e6f7ff; border: 1px solid #91d5ff; padding: 15px; border-radius: 6px; margin-bottom: 20px;'>
                <p style='margin: 0; font-size: 13px; color: #096dd9;'>
                    <strong>💡 Why share your location?</strong><br>
                    By sharing your current location, we can calculate the exact distance and estimated travel time to the rescue site. 
                    This helps us coordinate the fastest response and assign the nearest available rescuer.
                </p>
            </div>
        </div>

        <!-- Footer -->
        <div style='background: #f9f9f9; padding: 20px; text-align: center; border-top: 1px solid #e8e8e8;'>
            <p style='margin: 0 0 10px 0; color: #666; font-size: 14px;'>
                You're receiving this because you're a registered rescuer in this area.
            </p>
            <p style='margin: 0; color: #999; font-size: 12px;'>
                Pawchums Animal Rescue System © {DateTime.Now.Year}<br>
                Saving Lives, One Paw at a Time 🐾
            </p>
            <div style='margin-top: 15px;'>
                <a href='{frontendUrl}/rescuer/preferences' 
                   style='color: #1890ff; text-decoration: none; font-size: 12px;'>
                    Update Notification Preferences
                </a>
            </div>
        </div>
    </div>
</body>
</html>";
    }

    /// <summary>
    /// Notify all rescuers about a new request (without distance calculation)
    /// Used when request doesn't have location coordinates
    /// </summary>
    public async Task NotifyAllRescuersAsync(RequestRescue request)
    {
        try
        {
            _logger.LogInformation("Sending notifications to all rescuers for request {RequestId} (no location data)",
                request.Id);

            // Get all rescuer role users
            var rescuerRole = await _roleRepository.FindByNormalizedNameAsync("RESCUER");
            if (rescuerRole == null)
            {
                _logger.LogWarning("Rescuer role not found");
                return;
            }

            var rescuerUsers = await _userRepository.GetListByNormalizedRoleNameAsync(rescuerRole.NormalizedName);

            var frontendUrl = _configuration["App:FrontendUrl"] ?? "http://localhost:3000";

            foreach (var user in rescuerUsers)
            {
                if (string.IsNullOrEmpty(user.Email))
                    continue;

                try
                {
                    var token = await _tokenService.GenerateTokenAsync(request.Id, user.Id);
                    
                    var emailBody =  GetBasicRescueNotificationEmailBodyAsync(request, user, token, frontendUrl);

                    await _emailSender.SendAsync(
                        to: user.Email,
                        subject: $"🚨 New Rescue Request: {request.Title}",
                        body: emailBody,
                        isBodyHtml: true);

                    _logger.LogInformation("Notification sent to rescuer {UserId} ({Email})", user.Id, user.Email);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send notification to rescuer {UserId}", user.Id);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notifications for request {RequestId}", request.Id);
            throw;
        }
    }

    private string GetBasicRescueNotificationEmailBodyAsync(
    RequestRescue request,
    IdentityUser user,
    string locationToken,
    string frontendUrl)
    {
        var locationShareUrl = $"{frontendUrl}/share-location?token={locationToken}&requestId={request.Id}";
        var requestDetailsUrl = $"{frontendUrl}/rescue-requests/{request.Id}";

        var severityColor = request.Severity switch
        {
            "Critical" => "#ff4d4f",
            "High" => "#ff7a45",
            "Medium" => "#ffa940",
            "Low" => "#52c41a",
            _ => "#1890ff"
        };

        return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>New Rescue Request</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; background-color: #f5f5f5;'>
    <div style='max-width: 600px; margin: 20px auto; background: white; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 8px rgba(0,0,0,0.1);'>
        
        <!-- Header -->
        <div style='background: linear-gradient(135deg, #52c41a 0%, #73d13d 100%); color: white; padding: 30px 20px; text-align: center;'>
            <h1 style='margin: 0; font-size: 28px;'>🚨 New Rescue Request</h1>
            <p style='margin: 10px 0 0 0; font-size: 16px; opacity: 0.9;'>An animal needs your help!</p>
        </div>

        <!-- Greeting -->
        <div style='padding: 20px;'>
            <p style='margin: 0 0 20px 0; font-size: 16px;'>Hello <strong>{user.Name}</strong>,</p>
            <p style='margin: 0 0 20px 0;'>A new rescue request has been posted in your area:</p>
        </div>

        <!-- Request Details -->
        <div style='padding: 0 20px;'>
            <div style='background: #f9f9f9; padding: 20px; border-radius: 8px; margin-bottom: 20px;'>
                <h2 style='margin: 0 0 15px 0; color: #333; font-size: 22px;'>{request.Title}</h2>
                
                <div style='margin-bottom: 12px;'>
                    <span style='display: inline-block; background: {severityColor}; color: white; padding: 4px 12px; border-radius: 12px; font-size: 12px; font-weight: bold;'>
                        {request.Severity.ToUpper()} PRIORITY
                    </span>
                </div>

                <div style='margin-bottom: 10px;'>
                    <strong>📍 Location:</strong>
                    <p style='margin: 5px 0 0 0; color: #666;'>{request.Location}</p>
                </div>

                <div style='margin-bottom: 10px;'>
                    <strong>📝 Description:</strong>
                    <p style='margin: 5px 0 0 0; color: #666;'>{request.Description}</p>
                </div>

                <div style='margin-bottom: 10px;'>
                    <strong>📞 Contact:</strong>
                    <p style='margin: 5px 0 0 0; color: #666;'>
                        {request.ContactNo}
                        {(!string.IsNullOrEmpty(request.ContactName) ? $" ({request.ContactName})" : "")}
                    </p>
                </div>

                <div>
                    <strong>🕒 Reported:</strong>
                    <p style='margin: 5px 0 0 0; color: #666;'>{request.RequestDate:MMM dd, yyyy HH:mm}</p>
                </div>
            </div>

            <!-- Action Buttons -->
            <div style='margin-bottom: 30px;'>
                <h3 style='color: #333; font-size: 18px; margin-bottom: 15px;'>Take Action:</h3>
                
                <!-- Share Location Button - PRIMARY ACTION -->
                <a href='{locationShareUrl}' 
                   style='display: block; background: #52c41a; color: white; text-align: center; padding: 15px 20px; text-decoration: none; border-radius: 6px; font-weight: bold; font-size: 16px; margin-bottom: 10px;'>
                    📲 Share Your Current Location
                </a>

                <!-- View Details Button - SECONDARY ACTION -->
                <a href='{requestDetailsUrl}' 
                   style='display: block; background: white; color: #1890ff; text-align: center; padding: 12px 20px; text-decoration: none; border-radius: 6px; border: 2px solid #1890ff; font-weight: 500; margin-bottom: 10px;'>
                    👀 View Details in App
                </a>

                {(request.Latitude.HasValue && request.Longitude.HasValue ?
                    $@"<!-- Map Links -->
                <div style='display: grid; grid-template-columns: 1fr 1fr; gap: 10px;'>
                    <a href='https://www.openstreetmap.org/?mlat={request.Latitude}&mlon={request.Longitude}#map=15/{request.Latitude}/{request.Longitude}' 
                       style='display: block; background: #1890ff; color: white; text-align: center; padding: 10px; text-decoration: none; border-radius: 6px; font-size: 14px;'>
                        🗺️ OpenStreetMap
                    </a>
                    <a href='https://www.google.com/maps?q={request.Latitude},{request.Longitude}' 
                       style='display: block; background: #1890ff; color: white; text-align: center; padding: 10px; text-decoration: none; border-radius: 6px; font-size: 14px;'>
                        🗺️ Google Maps
                    </a>
                </div>" : "")}
            </div>

            <!-- Info Box -->
            <div style='background: #e6f7ff; border: 1px solid #91d5ff; padding: 15px; border-radius: 6px; margin-bottom: 20px;'>
                <p style='margin: 0; font-size: 13px; color: #096dd9;'>
                    <strong>💡 Why share your location?</strong><br>
                    By sharing your current location, we can calculate the exact distance to the rescue site. 
                    This helps us coordinate the fastest response and assign the nearest available rescuer.
                </p>
            </div>
        </div>

        <!-- Footer -->
        <div style='background: #f9f9f9; padding: 20px; text-align: center; border-top: 1px solid #e8e8e8;'>
            <p style='margin: 0 0 10px 0; color: #666; font-size: 14px;'>
                You're receiving this because you're a registered rescuer.
            </p>
            <p style='margin: 0; color: #999; font-size: 12px;'>
                Pawchums Animal Rescue System © {DateTime.Now.Year}<br>
                Saving Lives, One Paw at a Time 🐾
            </p>
        </div>
    </div>
</body>
</html>";
    }
}