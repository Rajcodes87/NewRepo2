using AnimalRescueSystem.Constants;
using AnimalRescueSystem.Dtos;
using AnimalRescueSystem.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pawchums.Entities.RescuerApplication;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace AnimalRescueSystem.RescuerApplications;

public class RescuerApplicationAppService : ApplicationService, IRescuerApplicationAppService
{
    private readonly IRepository<RescuerApplication, Guid> _rescuerApplicationRepository;
    private readonly IIdentityUserRepository _userRepository;
    private readonly IdentityUserManager _userManager;
    private readonly IIdentityRoleRepository _roleRepository;
    private readonly RescuerApplicationEmailService _emailService;
    private readonly ILogger<RescuerApplicationAppService> _logger;
    private readonly ICurrentUser _currentUser;
    private readonly IMapper _mapper;

    public RescuerApplicationAppService(
        IRepository<RescuerApplication, Guid> rescuerApplicationRepository,
        IIdentityUserRepository userRepository,
        IdentityUserManager userManager,
        IIdentityRoleRepository roleRepository,
        RescuerApplicationEmailService emailService,
        ILogger<RescuerApplicationAppService> logger,
        ICurrentUser currentUser,
        IMapper mapper)
    {
        _rescuerApplicationRepository = rescuerApplicationRepository;
        _userRepository = userRepository;
        _userManager = userManager;
        _roleRepository = roleRepository;
        _emailService = emailService;
        _logger = logger;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    /// <summary>
    /// Create a new rescuer application with identity card photo
    /// </summary>
    public async Task<ResponseDataDto<RescuerApplicationDto>> CreateAsync(CreateRescuerApplicationDto input)
    {
        try
        {
            _logger.LogInformation("RescuerApplicationAppService - CreateAsync: Started by User: {UserId}", _currentUser?.Id);

            if (!_currentUser.IsAuthenticated)
            {
                throw new UserFriendlyException("User must be authenticated to submit an application.", "401");
            }

            // Check if user already has a pending or approved application
            var existingApp = await _rescuerApplicationRepository.FirstOrDefaultAsync(
                x => x.UserId == _currentUser.Id.Value && (x.Status == RescuerApplicationConsts.ApplicationStatus.Pending || x.Status == RescuerApplicationConsts.ApplicationStatus.Approved));

            if (existingApp != null)
            {
                _logger.LogWarning("RescuerApplicationAppService - CreateAsync: User {UserId} already has an active application", _currentUser.Id);
                throw new UserFriendlyException("You already have a pending or approved application.", "400");
            }

            var user = await _userRepository.GetAsync(_currentUser.Id.Value);

            var application = new RescuerApplication
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Name = user.Name,
                Surname = user.Surname,
                PhoneNumber = user.PhoneNumber ?? "",
                IdentityCardPicture = input.IdentityCardPicture,
                Status = RescuerApplicationConsts.ApplicationStatus.Pending,
                ApplicationDate = DateTime.UtcNow,
                TenantId = CurrentTenant.Id
            };

            await _rescuerApplicationRepository.InsertAsync(application);

            _logger.LogInformation("RescuerApplicationAppService - CreateAsync: Application created for User: {UserId}", _currentUser.Id);

            // Send email notifications to all admins
            await _emailService.NotifyAdminsOfNewApplicationAsync(application);

            var response = _mapper.Map<RescuerApplication, RescuerApplicationDto>(application);

            return new ResponseDataDto<RescuerApplicationDto>
            {
                Code = 200,
                Success = true,
                Message = "Your identity card has been submitted successfully. Admins will review and verify it shortly. You'll be notified via email once the review is complete.",
                Data = response
            };
        }
        catch (UserFriendlyException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RescuerApplicationAppService - CreateAsync: Error creating application for User: {UserId}, Error: {ErrorMessage}", _currentUser?.Id, ex.Message);
            throw new UserFriendlyException(ErrorConsts.ServerError, "500");
        }
    }

    /// <summary>
    /// Get a list of rescuer applications (Admin only)
    /// </summary>
    [Authorize(Roles = "admin")]
    public async Task<ResponseDataDto<PagedResultDto<RescuerApplicationDto>>> GetListAsync(PagedAndSortedResultRequestDto input, RescuerApplicationFilter filter)
    {
        try
        {
            _logger.LogInformation("RescuerApplicationAppService - GetListAsync: Started by User: {UserId}", _currentUser?.Id);

            if (input.Sorting.IsNullOrWhiteSpace())
            {
                input.Sorting = "ApplicationDate DESC";
            }

            filter.SearchKeyword = filter.SearchKeyword?.Trim()?.ToLower();

            var query = await _rescuerApplicationRepository.GetQueryableAsync();

            var result = query
                .WhereIf(!string.IsNullOrWhiteSpace(filter.SearchKeyword), x =>
                    x.UserName.ToLower().Contains(filter.SearchKeyword) ||
                    x.Email.ToLower().Contains(filter.SearchKeyword) ||
                    x.Name.ToLower().Contains(filter.SearchKeyword) ||
                    x.Surname.ToLower().Contains(filter.SearchKeyword))
                .WhereIf(!string.IsNullOrWhiteSpace(filter.Status), x => x.Status == filter.Status)
                .OrderBy(input.Sorting)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount);

            var items = await AsyncExecuter.ToListAsync(result);
            var totalCount = await query.CountAsync();

            var dtos = _mapper.Map<RescuerApplicationDto[]>(items);

            _logger.LogInformation("RescuerApplicationAppService - GetListAsync: Retrieved {Count} applications", dtos.Length);

            return new ResponseDataDto<PagedResultDto<RescuerApplicationDto>>
            {
                Code = 200,
                Success = true,
                Message = CommonMessageConsts.DataRetrievedSuccessfully,
                Data = new PagedResultDto<RescuerApplicationDto>(totalCount, dtos)
            };
        }
        catch (UserFriendlyException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RescuerApplicationAppService - GetListAsync: Error retrieving applications by User: {UserId}", _currentUser?.Id);
            throw new UserFriendlyException(ErrorConsts.ServerError, "500");
        }
    }

    /// <summary>
    /// Get a rescuer application by ID (Admin or Application Owner)
    /// </summary>
    public async Task<ResponseDataDto<RescuerApplicationDto>> GetAsync([Required] Guid id)
    {
        try
        {
            _logger.LogInformation("RescuerApplicationAppService - GetAsync: Retrieving application {ApplicationId} by User: {UserId}", id, _currentUser?.Id);

            var application = await _rescuerApplicationRepository.GetAsync(id);

            if (application == null)
            {
                throw new UserFriendlyException(ErrorConsts.NotFound, "404");
            }

            // Check authorization: only admin or application owner can view
            if (!_currentUser.IsInRole("admin") && application.UserId != _currentUser.Id)
            {
                throw new UserFriendlyException("You are not authorized to view this application.", "403");
            }

            var response = _mapper.Map<RescuerApplication, RescuerApplicationDto>(application);

            return new ResponseDataDto<RescuerApplicationDto>
            {
                Code = 200,
                Success = true,
                Message = CommonMessageConsts.DataRetrievedSuccessfully,
                Data = response
            };
        }
        catch (UserFriendlyException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RescuerApplicationAppService - GetAsync: Error retrieving application {ApplicationId}", id);
            throw new UserFriendlyException(ErrorConsts.ServerError, "500");
        }
    }

    /// <summary>
    /// Review rescuer application - Approve or Reject (Admin only)
    /// </summary>
    [Authorize(Roles = "admin")]
    public async Task<ResponseDataDto<object>> ReviewAsync(ReviewRescuerApplicationDto input)
    {
        try
        {
            _logger.LogInformation("RescuerApplicationAppService - ReviewAsync: Reviewing application {ApplicationId} by Admin: {UserId}", input.ApplicationId, _currentUser?.Id);

            if (!Guid.TryParse(input.ApplicationId, out var applicationId))
            {
                throw new UserFriendlyException("Invalid application ID.", "400");
            }

            var application = await _rescuerApplicationRepository.GetAsync(applicationId);

            if (application == null)
            {
                throw new UserFriendlyException(ErrorConsts.NotFound, "404");
            }

            // Update application status
            application.Status = input.Status;
            application.ReviewedByUserId = _currentUser.Id;
            application.ReviewedDate = DateTime.UtcNow;
            application.ReviewNotes = input.ReviewNotes;

            await _rescuerApplicationRepository.UpdateAsync(application);

            _logger.LogInformation("RescuerApplicationAppService - ReviewAsync: Application {ApplicationId} reviewed as {Status}", applicationId, input.Status);

            // ✅ If approved, assign Rescuer role (THIS IS WHERE USER BECOMES A RESCUER)
            if (input.Status == RescuerApplicationConsts.ApplicationStatus.Approved)
            {
                var user = await _userRepository.GetAsync(application.UserId);
                var rescuerRole = await _roleRepository.FindByNormalizedNameAsync("RESCUER");

                if (rescuerRole == null)
                {
                    _logger.LogError("RescuerApplicationAppService - ReviewAsync: Rescuer role not found in the system");
                    throw new UserFriendlyException("Rescuer role is not configured. Please contact administrator.", "500");
                }

                // Add rescuer role if not already assigned
                var userRoles = await _userManager.GetRolesAsync(user);
                if (!userRoles.Contains(rescuerRole.Name))
                {
                    var roleResult = await _userManager.AddToRoleAsync(user, rescuerRole.Name);
                    if (!roleResult.Succeeded)
                    {
                        var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                        _logger.LogError("RescuerApplicationAppService - ReviewAsync: Failed to assign Rescuer role: {Errors}", errors);
                        throw new UserFriendlyException($"Failed to assign Rescuer role: {errors}", "500");
                    }
                }

                // Ensure user is active (should already be active from email verification, but double-check)
                if (!user.IsActive)
                {
                    user.SetIsActive(true);
                    await _userRepository.UpdateAsync(user);
                }

                _logger.LogInformation("RescuerApplicationAppService - ReviewAsync: User {UserId} assigned Rescuer role successfully", application.UserId);
            }

            // Send email notification to applicant
            await _emailService.NotifyApplicantOfReviewAsync(application);

            return new ResponseDataDto<object>
            {
                Code = 200,
                Success = true,
                Message = $"Application has been {input.Status.ToLower()} and the applicant has been notified via email."
            };
        }
        catch (UserFriendlyException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RescuerApplicationAppService - ReviewAsync: Error reviewing application");
            throw new UserFriendlyException(ErrorConsts.ServerError, "500");
        }
    }

    /// <summary>
    /// Get rescuer application by user ID (for logged-in user to check their own application status)
    /// </summary>
    public async Task<ResponseDataDto<RescuerApplicationDto>> GetByUserIdAsync(Guid userId)
    {
        try
        {
            _logger.LogInformation("RescuerApplicationAppService - GetByUserIdAsync: Retrieving application for User: {UserId}", userId);

            var application = await _rescuerApplicationRepository.FirstOrDefaultAsync(x => x.UserId == userId);

            if (application == null)
            {
                return new ResponseDataDto<RescuerApplicationDto>
                {
                    Code = 404,
                    Success = false,
                    Message = "No application found for this user.",
                    Data = null
                };
            }

            var response = _mapper.Map<RescuerApplication, RescuerApplicationDto>(application);

            return new ResponseDataDto<RescuerApplicationDto>
            {
                Code = 200,
                Success = true,
                Message = CommonMessageConsts.DataRetrievedSuccessfully,
                Data = response
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RescuerApplicationAppService - GetByUserIdAsync: Error retrieving application for User: {UserId}", userId);
            throw new UserFriendlyException(ErrorConsts.ServerError, "500");
        }
    }
}