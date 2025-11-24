using System;
using System.Threading.Tasks;
using AnimalRescueSystem.Dtos;
using Volo.Abp.Application.Services;

namespace AnimalRescueSystem.RescuerProfiles;

public interface IRescuerProfileAppService : IApplicationService
{
    /// <summary>
    /// Get rescuer profile by user ID
    /// </summary>
    Task<ResponseDataDto<RescuerProfileDto>> GetByUserIdAsync(Guid userId);

    /// <summary>
    /// Get current user's rescuer profile
    /// </summary>
    Task<ResponseDataDto<RescuerProfileDto>> GetMyProfileAsync();

    /// <summary>
    /// Create or update rescuer profile
    /// </summary>
    Task<ResponseDataDto<RescuerProfileDto>> CreateOrUpdateAsync(CreateUpdateRescuerProfileDto input);

    /// <summary>
    /// Share location via email link (token-based)
    /// </summary>
    Task<ResponseDataDto<object>> ShareLocationAsync(ShareLocationDto input);

    /// <summary>
    /// Get nearest rescuers to a rescue request
    /// </summary>
    Task<ResponseDataDto<RescuerDistanceDto[]>> GetNearestRescuersAsync(Guid requestRescueId, int maxResults = 10);
}