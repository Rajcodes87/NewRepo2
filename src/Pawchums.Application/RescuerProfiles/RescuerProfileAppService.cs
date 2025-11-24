using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AnimalRescueSystem.Constants;
using AnimalRescueSystem.Dtos;
using AnimalRescueSystem.RescuerProfiles;
using Microsoft.Extensions.Logging;
using Pawchums.Entities.RescuerProfile;
using Pawchums.Services;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace AnimalRescueSystem.RescuerProfiles;

public class RescuerProfileAppService : ApplicationService, IRescuerProfileAppService
{
    private readonly IRepository<RescuerProfile, Guid> _rescuerProfileRepository;
    private readonly IIdentityUserRepository _userRepository;
    private readonly IIdentityRoleRepository _roleRepository;
    private readonly DistanceCalculationService _distanceService;
    private readonly IRepository<Pawchums.Entities.RequestRescues.RequestRescue, Guid> _requestRescueRepository;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<RescuerProfileAppService> _logger;

    public RescuerProfileAppService(
        IRepository<RescuerProfile, Guid> rescuerProfileRepository,
        IIdentityUserRepository userRepository,
        IIdentityRoleRepository roleRepository,
        DistanceCalculationService distanceService,
        IRepository<Pawchums.Entities.RequestRescues.RequestRescue, Guid> requestRescueRepository,
        ICurrentUser currentUser,
        ILogger<RescuerProfileAppService> logger)
    {
        _rescuerProfileRepository = rescuerProfileRepository;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _distanceService = distanceService;
        _requestRescueRepository = requestRescueRepository;
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <summary>
    /// Get rescuer profile by user ID
    /// </summary>
    public async Task<ResponseDataDto<RescuerProfileDto>> GetByUserIdAsync(Guid userId)
    {
        try
        {
            _logger.LogInformation("RescuerProfileAppService - GetByUserIdAsync: Fetching profile for user {UserId}", userId);

            var profile = await _rescuerProfileRepository.FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                return new ResponseDataDto<RescuerProfileDto>
                {
                    Code = 404,
                    Success = false,
                    Message = "Rescuer profile not found.",
                    Data = null
                };
            }

            var dto = ObjectMapper.Map<RescuerProfile, RescuerProfileDto>(profile);

            return new ResponseDataDto<RescuerProfileDto>
            {
                Code = 200,
                Success = true,
                Message = CommonMessageConsts.DataRetrievedSuccessfully,
                Data = dto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RescuerProfileAppService - GetByUserIdAsync: Error fetching profile for user {UserId}", userId);
            throw new UserFriendlyException(ErrorConsts.ServerError, "500");
        }
    }

    /// <summary>
    /// Get current user's rescuer profile
    /// </summary>
    public async Task<ResponseDataDto<RescuerProfileDto>> GetMyProfileAsync()
    {
        try
        {
            if (!_currentUser.IsAuthenticated)
            {
                throw new UserFriendlyException("User must be authenticated.", "401");
            }

            return await GetByUserIdAsync(_currentUser.Id.Value);
        }
        catch (UserFriendlyException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RescuerProfileAppService - GetMyProfileAsync: Error fetching profile");
            throw new UserFriendlyException(ErrorConsts.ServerError, "500");
        }
    }

    /// <summary>
    /// Create or update rescuer profile
    /// </summary>
    public async Task<ResponseDataDto<RescuerProfileDto>> CreateOrUpdateAsync(CreateUpdateRescuerProfileDto input)
    {
        try
        {
            if (!_currentUser.IsAuthenticated)
            {
                throw new UserFriendlyException("User must be authenticated.", "401");
            }

            var userId = _currentUser.Id.Value;

            _logger.LogInformation("RescuerProfileAppService - CreateOrUpdateAsync: Creating/updating profile for user {UserId}", userId);

            var existingProfile = await _rescuerProfileRepository.FirstOrDefaultAsync(p => p.UserId == userId);

            RescuerProfile profile;

            if (existingProfile == null)
            {
                // Create new profile
                profile = new RescuerProfile
                {
                    UserId = userId,
                    TenantId = CurrentTenant.Id
                };

                ObjectMapper.Map(input, profile);
                profile.LocationUpdatedAt = DateTime.UtcNow;

                await _rescuerProfileRepository.InsertAsync(profile);
                _logger.LogInformation("RescuerProfileAppService - CreateOrUpdateAsync: Created new profile for user {UserId}", userId);
            }
            else
            {
                // Update existing profile
                ObjectMapper.Map(input, existingProfile);
                existingProfile.LocationUpdatedAt = DateTime.UtcNow;

                await _rescuerProfileRepository.UpdateAsync(existingProfile);
                profile = existingProfile;
                _logger.LogInformation("RescuerProfileAppService - CreateOrUpdateAsync: Updated profile for user {UserId}", userId);
            }

            var dto = ObjectMapper.Map<RescuerProfile, RescuerProfileDto>(profile);

            return new ResponseDataDto<RescuerProfileDto>
            {
                Code = 200,
                Success = true,
                Message = existingProfile == null
                    ? "Profile created successfully."
                    : "Profile updated successfully.",
                Data = dto
            };
        }
        catch (UserFriendlyException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RescuerProfileAppService - CreateOrUpdateAsync: Error creating/updating profile");
            throw new UserFriendlyException(ErrorConsts.ServerError, "500");
        }
    }

    /// <summary>
    /// Share location via email link (token-based)
    /// This allows rescuers to share their location when they click the email link
    /// </summary>
    public async Task<ResponseDataDto<object>> ShareLocationAsync(ShareLocationDto input)
    {
        try
        {
            _logger.LogInformation("RescuerProfileAppService - ShareLocationAsync: Sharing location for request {RequestId}",
                input.RequestRescueId);

            // Validate request exists
            var request = await _requestRescueRepository.GetAsync(input.RequestRescueId);
            if (request == null)
            {
                throw new UserFriendlyException("Rescue request not found.", "404");
            }

            // TODO: Validate token (implement token validation logic)
            // For now, assuming token is valid if provided

            if (!_currentUser.IsAuthenticated)
            {
                throw new UserFriendlyException("User must be authenticated.", "401");
            }

            var userId = _currentUser.Id.Value;

            // Create or update profile with location
            var profile = await _rescuerProfileRepository.FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                profile = new RescuerProfile
                {
                    UserId = userId,
                    TenantId = CurrentTenant.Id,
                    Latitude = input.Latitude,
                    Longitude = input.Longitude,
                    CurrentLocation = input.CurrentLocation,
                    IsLocationShared = true,
                    LocationUpdatedAt = DateTime.UtcNow
                };

                await _rescuerProfileRepository.InsertAsync(profile);
            }
            else
            {
                profile.Latitude = input.Latitude;
                profile.Longitude = input.Longitude;
                profile.CurrentLocation = input.CurrentLocation;
                profile.IsLocationShared = true;
                profile.LocationUpdatedAt = DateTime.UtcNow;

                await _rescuerProfileRepository.UpdateAsync(profile);
            }

            // Calculate distance to rescue location
            double distance = 0;
            if (request.Latitude.HasValue && request.Longitude.HasValue)
            {
                distance = _distanceService.CalculateDistance(
                    request.Latitude.Value,
                    request.Longitude.Value,
                    input.Latitude,
                    input.Longitude);
            }

            _logger.LogInformation("RescuerProfileAppService - ShareLocationAsync: Location shared. Distance to rescue: {Distance} km",
                distance);

            return new ResponseDataDto<object>
            {
                Code = 200,
                Success = true,
                Message = $"Location shared successfully. You are {distance:F1} km away from the rescue location.",
                Data = new { distance = distance, distanceDisplay = $"{distance:F1} km" }
            };
        }
        catch (UserFriendlyException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RescuerProfileAppService - ShareLocationAsync: Error sharing location");
            throw new UserFriendlyException(ErrorConsts.ServerError, "500");
        }
    }

    /// <summary>
    /// Get nearest rescuers to a rescue request
    /// Uses Haversine formula and K-nearest neighbors algorithm
    /// </summary>
    public async Task<ResponseDataDto<RescuerDistanceDto[]>> GetNearestRescuersAsync(
        Guid requestRescueId,
        int maxResults = 10)
    {
        try
        {
            _logger.LogInformation("RescuerProfileAppService - GetNearestRescuersAsync: Finding nearest rescuers for request {RequestId}",
                requestRescueId);

            // Get rescue request
            var request = await _requestRescueRepository.GetAsync(requestRescueId);
            if (request == null)
            {
                throw new UserFriendlyException("Rescue request not found.", "404");
            }

            if (!request.Latitude.HasValue || !request.Longitude.HasValue)
            {
                throw new UserFriendlyException("Rescue request does not have location coordinates.", "400");
            }

            // Get all rescuers with the Rescuer role
            var rescuerRole = await _roleRepository.FindByNormalizedNameAsync("RESCUER");
            if (rescuerRole == null)
            {
                _logger.LogWarning("RescuerProfileAppService - GetNearestRescuersAsync: Rescuer role not found");
                return new ResponseDataDto<RescuerDistanceDto[]>
                {
                    Code = 200,
                    Success = true,
                    Message = "No rescuers found.",
                    Data = Array.Empty<RescuerDistanceDto>()
                };
            }

            var rescuerUsers = await _userRepository.GetListByNormalizedRoleNameAsync(rescuerRole.NormalizedName);

            // Get profiles for these rescuers
            var rescuerUserIds = rescuerUsers.Select(u => u.Id).ToList();
            var rescuerProfiles = await _rescuerProfileRepository.GetListAsync(
                p => rescuerUserIds.Contains(p.UserId) && p.IsLocationShared);

            // Map to RescuerLocationDto
            var rescuerLocations = rescuerProfiles
                .Select(p =>
                {
                    var user = rescuerUsers.FirstOrDefault(u => u.Id == p.UserId);
                    return new RescuerLocationDto
                    {
                        RescuerId = p.UserId,
                        RescuerName = user != null ? $"{user.Name} {user.Surname}".Trim() : "Unknown",
                        Email = user?.Email ?? "",
                        Latitude = p.Latitude,
                        Longitude = p.Longitude
                    };
                })
                .ToList();

            // Calculate distances using our algorithm
            var nearestRescuers = _distanceService.FindNearestRescuers(
                request.Latitude.Value,
                request.Longitude.Value,
                rescuerLocations,
                maxResults);

            _logger.LogInformation("RescuerProfileAppService - GetNearestRescuersAsync: Found {Count} nearest rescuers",
                nearestRescuers.Count);

            return new ResponseDataDto<RescuerDistanceDto[]>
            {
                Code = 200,
                Success = true,
                Message = $"Found {nearestRescuers.Count} nearest rescuers.",
                Data = nearestRescuers.ToArray()
            };
        }
        catch (UserFriendlyException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RescuerProfileAppService - GetNearestRescuersAsync: Error finding nearest rescuers");
            throw new UserFriendlyException(ErrorConsts.ServerError, "500");
        }
    }
}