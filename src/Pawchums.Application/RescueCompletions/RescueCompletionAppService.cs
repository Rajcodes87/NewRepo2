using AnimalRescueSystem.Constants;
using AnimalRescueSystem.Dtos;
using AnimalRescueSystem.Entities.RequestRescues;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pawchums.Entities.RequestRescues;
using Pawchums.RescueCompletions;
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
using Volo.Abp.Uow;
using Volo.Abp.Users;

namespace AnimalRescueSystem.RescueCompletions;

[Authorize(Roles = "Rescuer,admin")] // Only Rescuers and admins can access
public class RescueCompletionAppService : ApplicationService, IRescueCompletionAppService
{
    private readonly IRepository<RescueCompletion, Guid> _rescueCompletionRepository;
    private readonly IRepository<RequestRescue, Guid> _requestRescueRepository;
    private readonly IRepository<RescueInitiation, Guid> _rescueInitiationRepository;
    private readonly IRepository<IdentityUser, Guid> _userRepository;
    private readonly ILogger<RescueCompletionAppService> _logger;
    private readonly ICurrentUser _currentUser;
    private readonly IMapper _mapper;

    public RescueCompletionAppService(
        IRepository<RescueCompletion, Guid> rescueCompletionRepository,
        IRepository<RequestRescue, Guid> requestRescueRepository,
        IRepository<RescueInitiation, Guid> rescueInitiationRepository,
        IRepository<IdentityUser, Guid> userRepository,
        ILogger<RescueCompletionAppService> logger,
        ICurrentUser currentUser,
        IMapper mapper)
    {
        _rescueCompletionRepository = rescueCompletionRepository;
        _requestRescueRepository = requestRescueRepository;
        _rescueInitiationRepository = rescueInitiationRepository;
        _userRepository = userRepository;
        _logger = logger;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    #region Public Methods

    /// <summary>
    /// Create a new Rescue Completion
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="UserFriendlyException"></exception>
    //[Authorize(AnimalRescueSystemPermissions.RescueCompletions.Create)]
    public async Task<ResponseDataDto<RescueCompletionResponseDto>> CreateAsync(CreateRescueCompletionDto input)
    {
        try
        {
            _logger.LogInformation("RescueCompletionAppService - CreateAsync: Started by User: {UserId}", _currentUser?.Id);

            await ValidateCreateInput(input);

            var rescueCompletion = _mapper.Map<CreateRescueCompletionDto, RescueCompletion>(input);
            rescueCompletion.CompletedByRescuerId = _currentUser.Id.Value;
            rescueCompletion.IsVerified = false;

            await _rescueCompletionRepository.InsertAsync(rescueCompletion);

            // Update RequestRescue status to Completed
            await UpdateRequestRescueStatusAsync(input.RequestRescueId, RequestRescueConsts.RequestStatus.Completed);

            var rescueCompletionDto = _mapper.Map<RescueCompletion, RescueCompletionDto>(rescueCompletion);
            _logger.LogInformation("RescueCompletionAppService - CreateAsync: {Message} by User: {UserId}", CommonMessageConsts.DataSavedSuccessfully, _currentUser?.Id);

            var response = new ResponseDataDto<RescueCompletionResponseDto>
            {
                Code = 200,
                Success = true,
                Message = CommonMessageConsts.DataSavedSuccessfully,
                Data = new RescueCompletionResponseDto
                {
                    Id = rescueCompletionDto.Id
                }
            };

            return response;
        }
        catch (UserFriendlyException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RescueCompletionAppService - CreateAsync: Requested by User: {UserId}, Error: {ErrorMessage}", _currentUser?.Id, ErrorConsts.ServerError);
            throw new UserFriendlyException(ErrorConsts.ServerError, "500");
        }
    }

    /// <summary>
    /// Update an existing Rescue Completion
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="UserFriendlyException"></exception>
    //[Authorize(AnimalRescueSystemPermissions.RescueCompletions.Update)]
    public async Task<ResponseDataDto<RescueCompletionResponseDto>> UpdateAsync([Required(ErrorMessage = "Id is required.")] Guid id, UpdateRescueCompletionDto input)
    {
        try
        {
            _logger.LogInformation("RescueCompletionAppService - UpdateAsync: Started by User: {UserId}", _currentUser?.Id);

            var rescueCompletion = await GetRescueCompletionAsync(id);

            // Validate that only the rescuer can update their own completion
            if (rescueCompletion.CompletedByRescuerId != _currentUser.Id)
            {
                _logger.LogWarning("RescueCompletionAppService - UpdateAsync: Unauthorized update attempt by User: {UserId} for Completion: {CompletionId}", _currentUser?.Id, id);
                throw new UserFriendlyException("You can only update your own completions.", "403");
            }

            // Cannot update if already verified
            if (rescueCompletion.IsVerified)
            {
                _logger.LogWarning("RescueCompletionAppService - UpdateAsync: Attempt to update verified completion: {CompletionId}", id);
                throw new UserFriendlyException("Cannot update a verified completion.", "400");
            }

            _mapper.Map(input, rescueCompletion);

            var rescueCompletionDto = _mapper.Map<RescueCompletionDto>(rescueCompletion);

            await _rescueCompletionRepository.UpdateAsync(rescueCompletion);

            _logger.LogInformation("RescueCompletionAppService - UpdateAsync: {Message} by User: {UserId}", CommonMessageConsts.DataSavedSuccessfully, _currentUser?.Id);

            var response = new ResponseDataDto<RescueCompletionResponseDto>
            {
                Code = 200,
                Success = true,
                Message = CommonMessageConsts.DataSavedSuccessfully,
                Data = new RescueCompletionResponseDto
                {
                    Id = rescueCompletionDto.Id
                }
            };

            return response;
        }
        catch (UserFriendlyException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RescueCompletionAppService - UpdateAsync: Requested by User: {UserId}, Error: {ErrorMessage}", _currentUser?.Id, ErrorConsts.ServerError);
            throw new UserFriendlyException(ErrorConsts.ServerError, "500");
        }
    }

    /// <summary>
    /// Get a list of Rescue Completions
    /// </summary>
    /// <param name="input"></param>
    /// <param name="filter"></param>
    /// <returns></returns>
    /// <exception cref="UserFriendlyException"></exception>
    //[Authorize(AnimalRescueSystemPermissions.RescueCompletions.Default)]
    public async Task<ResponseDataDto<PagedResultDto<RescueCompletionDto>>> GetListAsync(PagedAndSortedResultRequestDto input, RescueCompletionFilter filter)
    {
        try
        {
            _logger.LogInformation("RescueCompletionAppService - GetListAsync: Started by User: {UserId}", _currentUser?.Id);

            if (input.Sorting.IsNullOrWhiteSpace())
            {
                input.Sorting = "CompletionDate DESC";
            }

            filter.SearchKeyword = filter.SearchKeyword?.Trim()?.ToLower();

            var rescueCompletions = await _rescueCompletionRepository.GetQueryableAsync();
            var requestRescues = await _requestRescueRepository.GetQueryableAsync();
            var users = await _userRepository.GetQueryableAsync();

            var query = (from rc in rescueCompletions
                         join rr in requestRescues on rc.RequestRescueId equals rr.Id
                         join u in users on rc.CompletedByRescuerId equals u.Id
                         join verifiedBy in users on rc.VerifiedByUserId equals verifiedBy.Id into verifiedByGroup
                         from verifiedBy in verifiedByGroup.DefaultIfEmpty()
                         select new RescueCompletionDto
                         {
                             Id = rc.Id,
                             RequestRescueId = rc.RequestRescueId,
                             RequestTitle = rr.Title,
                             RequestLocation = rr.Location,
                             CompletionProofPicture = rc.CompletionProofPicture,
                             CompletionDate = rc.CompletionDate,
                             CompletionDescription = rc.CompletionDescription,
                             CompletedByRescuerId = rc.CompletedByRescuerId,
                             CompletedByRescuerName = u.Name,
                             CompletedByRescuerEmail = u.Email,
                             IsVerified = rc.IsVerified,
                             VerifiedByUserId = rc.VerifiedByUserId,
                             VerifiedByUserName = verifiedBy != null ? verifiedBy.Name : null,
                             VerifiedDate = rc.VerifiedDate,
                             VerificationNotes = rc.VerificationNotes,
                             CreationTime = rc.CreationTime
                         })
                        .WhereIf(!string.IsNullOrWhiteSpace(filter.SearchKeyword), x =>
                                                        x.RequestTitle.ToLower().Contains(filter.SearchKeyword) ||
                                                        x.RequestLocation.ToLower().Contains(filter.SearchKeyword) ||
                                                        (x.CompletedByRescuerName != null && x.CompletedByRescuerName.ToLower().Contains(filter.SearchKeyword))
                                                        )
                        .WhereIf(filter.IsVerified.HasValue, x => x.IsVerified == filter.IsVerified.Value)
                        .WhereIf(filter.RequestRescueId.HasValue, x => x.RequestRescueId == filter.RequestRescueId.Value)
                        .WhereIf(filter.CompletedByRescuerId.HasValue, x => x.CompletedByRescuerId == filter.CompletedByRescuerId.Value)
                        .WhereIf(filter.CompletionDateFrom.HasValue, x => x.CompletionDate >= filter.CompletionDateFrom.Value)
                        .WhereIf(filter.CompletionDateTo.HasValue, x => x.CompletionDate <= filter.CompletionDateTo.Value);

            var dtos = await AsyncExecuter.ToListAsync(query
                .OrderBy(input.Sorting)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount));

            var totalCount = await query.CountAsync();

            _logger.LogInformation("RescueCompletionAppService - GetListAsync: {Message} by User: {UserId}", CommonMessageConsts.DataRetrievedSuccessfully, _currentUser?.Id);

            var result = new PagedResultDto<RescueCompletionDto>(totalCount, dtos);
            return new ResponseDataDto<PagedResultDto<RescueCompletionDto>>()
            {
                Success = true,
                Code = 200,
                Message = CommonMessageConsts.DataRetrievedSuccessfully,
                Data = result
            };
        }
        catch (UserFriendlyException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RescueCompletionAppService - GetListAsync: Requested by User: {UserId}, Error: {ErrorMessage}", _currentUser?.Id, ErrorConsts.ServerError);
            throw new UserFriendlyException(ErrorConsts.InternalServerError, "500");
        }
    }

    /// <summary>
    /// Get a Rescue Completion by its id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="UserFriendlyException"></exception>
    //[Authorize(AnimalRescueSystemPermissions.RescueCompletions.Default)]
    public async Task<ResponseDataDto<RescueCompletionDto>> GetAsync([Required(ErrorMessage = "Id is required.")] Guid id)
    {
        try
        {
            _logger.LogInformation("RescueCompletionAppService - GetAsync: Started by User: {UserId}", _currentUser.Id);

            var rescueCompletion = await GetRescueCompletionAsync(id);
            var response = _mapper.Map<RescueCompletion, RescueCompletionDto>(rescueCompletion);

            // Populate related data
            var requestRescue = await _requestRescueRepository.GetAsync(rescueCompletion.RequestRescueId);
            var rescuer = await _userRepository.GetAsync(rescueCompletion.CompletedByRescuerId);

            response.RequestTitle = requestRescue.Title;
            response.RequestLocation = requestRescue.Location;
            response.CompletedByRescuerName = rescuer.Name;
            response.CompletedByRescuerEmail = rescuer.Email;

            if (rescueCompletion.VerifiedByUserId.HasValue)
            {
                var verifiedByUser = await _userRepository.GetAsync(rescueCompletion.VerifiedByUserId.Value);
                response.VerifiedByUserName = verifiedByUser.Name;
            }

            _logger.LogInformation("RescueCompletionAppService - GetAsync: {Message} by User: {UserId}", CommonMessageConsts.DataRetrievedSuccessfully, _currentUser.Id);

            return new ResponseDataDto<RescueCompletionDto>
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
            _logger.LogError(ex, "RescueCompletionAppService - GetAsync: Requested by User: {UserId}, Error: {ErrorMessage}", _currentUser.Id, ErrorConsts.ServerError);
            throw new UserFriendlyException(ErrorConsts.ServerError, "500");
        }
    }

    /// <summary>
    /// Get Rescue Completion by Request Id
    /// </summary>
    /// <param name="requestId"></param>
    /// <returns></returns>
    /// <exception cref="UserFriendlyException"></exception>
    //[Authorize(AnimalRescueSystemPermissions.RescueCompletions.Default)]
    public async Task<ResponseDataDto<RescueCompletionDto>> GetByRequestIdAsync([Required(ErrorMessage = "Request Id is required.")] Guid requestId)
    {
        try
        {
            _logger.LogInformation("RescueCompletionAppService - GetByRequestIdAsync: Started for Request: {RequestId} by User: {UserId}", requestId, _currentUser?.Id);

            var rescueCompletion = await _rescueCompletionRepository.FindAsync(x => x.RequestRescueId == requestId);

            if (rescueCompletion == null)
            {
                _logger.LogInformation("RescueCompletionAppService - GetByRequestIdAsync: No completion found for Request: {RequestId}", requestId);
                throw new UserFriendlyException("Rescue completion not found for this request.", "404");
            }

            return await GetAsync(rescueCompletion.Id);
        }
        catch (UserFriendlyException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RescueCompletionAppService - GetByRequestIdAsync: Requested by User: {UserId}, Error: {ErrorMessage}", _currentUser?.Id, ErrorConsts.ServerError);
            throw new UserFriendlyException(ErrorConsts.ServerError, "500");
        }
    }

    /// <summary>
    /// Delete a Rescue Completion by its id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="UserFriendlyException"></exception>
    //[Authorize(AnimalRescueSystemPermissions.RescueCompletions.Delete)]
    public async Task<ResponseDataDto<RescueCompletionResponseDto>> DeleteAsync([Required(ErrorMessage = "Id is required.")] Guid id)
    {
        try
        {
            _logger.LogInformation("RescueCompletionAppService - DeleteAsync: Started by User: {UserId}", _currentUser.Id);

            var rescueCompletion = await GetRescueCompletionAsync(id);

            // Validate that only the rescuer can delete their own completion
            if (rescueCompletion.CompletedByRescuerId != _currentUser.Id)
            {
                _logger.LogWarning("RescueCompletionAppService - DeleteAsync: Unauthorized delete attempt by User: {UserId} for Completion: {CompletionId}", _currentUser?.Id, id);
                throw new UserFriendlyException("You can only delete your own completions.", "403");
            }

            // Cannot delete if verified
            if (rescueCompletion.IsVerified)
            {
                _logger.LogWarning("RescueCompletionAppService - DeleteAsync: Attempt to delete verified completion: {CompletionId}", id);
                throw new UserFriendlyException("Cannot delete a verified completion.", "400");
            }

            await _rescueCompletionRepository.DeleteAsync(id);

            // Revert RequestRescue status back to InProgress
            await UpdateRequestRescueStatusAsync(rescueCompletion.RequestRescueId, RequestRescueConsts.RequestStatus.InProgress);

            _logger.LogInformation("RescueCompletionAppService - DeleteAsync: {message} by User: {UserId}", CommonMessageConsts.DataDeletedSuccessfully, _currentUser.Id);

            var response = new ResponseDataDto<RescueCompletionResponseDto>
            {
                Code = 200,
                Success = true,
                Message = CommonMessageConsts.DataDeletedSuccessfully,
                Data = new RescueCompletionResponseDto
                {
                    Id = id
                }
            };

            return response;
        }
        catch (UserFriendlyException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RescueCompletionAppService - DeleteAsync: Requested by User: {UserId}, Error: {ErrorMessage}", _currentUser.Id, ErrorConsts.ServerError);
            throw new UserFriendlyException(ErrorConsts.ServerError, "500");
        }
    }

    /// <summary>
    /// Verify a Rescue Completion (Admin operation)
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="UserFriendlyException"></exception>
    //[Authorize(AnimalRescueSystemPermissions.RescueCompletions.Verify)]
    [Authorize(Roles = "admin")]
    public async Task<ResponseDataDto<object>> VerifyCompletionAsync([Required(ErrorMessage = "Id is required.")] Guid id, VerifyCompletionDto input)
    {
        try
        {
            _logger.LogInformation("RescueCompletionAppService - VerifyCompletionAsync: Verify Rescue Completion requested for ID: {CompletionId} by User: {UserId}", id, _currentUser?.Id);

            using var uow = UnitOfWorkManager.Begin();

            var rescueCompletion = await _rescueCompletionRepository.FindAsync(id) ?? throw new UserFriendlyException("Rescue Completion not found.", "404");

            if (rescueCompletion.IsVerified)
            {
                _logger.LogWarning("RescueCompletionAppService - VerifyCompletionAsync: Attempted to verify already verified completion with ID: {CompletionId}", id);
                throw new UserFriendlyException("Completion is already verified.", "400");
            }

            rescueCompletion.IsVerified = true;
            rescueCompletion.VerifiedByUserId = _currentUser.Id;
            rescueCompletion.VerifiedDate = DateTime.UtcNow;
            rescueCompletion.VerificationNotes = input.VerificationNotes;

            await _rescueCompletionRepository.UpdateAsync(rescueCompletion);
            await uow.CompleteAsync();

            _logger.LogInformation("RescueCompletionAppService - VerifyCompletionAsync: Rescue Completion verified successfully for ID: {CompletionId}", id);

            return new ResponseDataDto<object>
            {
                Code = 200,
                Success = true,
                Message = "Rescue Completion verified successfully.",
            };
        }
        catch (UserFriendlyException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RescueCompletionAppService - VerifyCompletionAsync: Error occurred while verifying Rescue Completion with ID: {CompletionId}", id);
            throw new UserFriendlyException(ErrorConsts.ServerError, "500");
        }
    }

    /// <summary>
    /// Unverify a Rescue Completion (Admin operation)
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="UserFriendlyException"></exception>
    //[Authorize(AnimalRescueSystemPermissions.RescueCompletions.Unverify)]
    [Authorize(Roles = "admin")]
    public async Task<ResponseDataDto<object>> UnverifyCompletionAsync([Required(ErrorMessage = "Id is required.")] Guid id)
    {
        try
        {
            _logger.LogInformation("RescueCompletionAppService - UnverifyCompletionAsync: Unverify Rescue Completion requested for ID: {CompletionId} by User: {UserId}", id, _currentUser?.Id);

            using var uow = UnitOfWorkManager.Begin();

            var rescueCompletion = await _rescueCompletionRepository.FindAsync(id) ?? throw new UserFriendlyException("Rescue Completion not found.", "404");

            if (!rescueCompletion.IsVerified)
            {
                _logger.LogWarning("RescueCompletionAppService - UnverifyCompletionAsync: Attempted to unverify non-verified completion with ID: {CompletionId}", id);
                throw new UserFriendlyException("Completion is not verified.", "400");
            }

            rescueCompletion.IsVerified = false;
            rescueCompletion.VerifiedByUserId = null;
            rescueCompletion.VerifiedDate = null;
            rescueCompletion.VerificationNotes = null;

            await _rescueCompletionRepository.UpdateAsync(rescueCompletion);
            await uow.CompleteAsync();

            _logger.LogInformation("RescueCompletionAppService - UnverifyCompletionAsync: Rescue Completion unverified successfully for ID: {CompletionId}", id);

            return new ResponseDataDto<object>
            {
                Code = 200,
                Success = true,
                Message = "Rescue Completion unverified successfully.",
            };
        }
        catch (UserFriendlyException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RescueCompletionAppService - UnverifyCompletionAsync: Error occurred while unverifying Rescue Completion with ID: {CompletionId}", id);
            throw new UserFriendlyException(ErrorConsts.ServerError, "500");
        }
    }

    /// <summary>
    /// Get all completions for the current user (rescuer)
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="UserFriendlyException"></exception>
    //[Authorize(AnimalRescueSystemPermissions.RescueCompletions.Default)]
    public async Task<ResponseDataDto<PagedResultDto<RescueCompletionDto>>> GetMyCompletionsAsync(PagedAndSortedResultRequestDto input)
    {
        try
        {
            _logger.LogInformation("RescueCompletionAppService - GetMyCompletionsAsync: Started by User: {UserId}", _currentUser?.Id);

            if (!_currentUser.Id.HasValue)
            {
                throw new UserFriendlyException("User must be authenticated.", "401");
            }

            var filter = new RescueCompletionFilter
            {
                CompletedByRescuerId = _currentUser.Id.Value
            };

            return await GetListAsync(input, filter);
        }
        catch (UserFriendlyException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RescueCompletionAppService - GetMyCompletionsAsync: Requested by User: {UserId}, Error: {ErrorMessage}", _currentUser?.Id, ErrorConsts.ServerError);
            throw new UserFriendlyException(ErrorConsts.ServerError, "500");
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Validate the input for creating a Rescue Completion
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private async Task ValidateCreateInput(CreateRescueCompletionDto input)
    {
        // Check if request exists
        var requestRescue = await _requestRescueRepository.FindAsync(input.RequestRescueId);
        if (requestRescue == null)
        {
            _logger.LogInformation("RescueCompletionAppService - ValidateCreateInput: Request Rescue not found: {RequestId}", input.RequestRescueId);
            throw new UserFriendlyException("Request Rescue not found.", "404");
        }

        // Check if request is in InProgress status
        if (requestRescue.Status != RequestRescueConsts.RequestStatus.InProgress)
        {
            _logger.LogInformation("RescueCompletionAppService - ValidateCreateInput: Request Rescue is not in progress: {RequestId}", input.RequestRescueId);
            throw new UserFriendlyException("Only requests in progress can be completed.", "400");
        }

        // Check if user is authenticated
        if (!_currentUser.Id.HasValue)
        {
            _logger.LogInformation("RescueCompletionAppService - ValidateCreateInput: User not authenticated");
            throw new UserFriendlyException("User must be authenticated to complete rescue.", "401");
        }

        // Check if the current user is the selected rescuer
        var acceptedInitiation = await _rescueInitiationRepository.FindAsync(x =>
            x.RequestRescueId == input.RequestRescueId &&
            x.Status == RescueInitiationConsts.InitiationStatus.Accepted &&
            x.IsSelected);

        if (acceptedInitiation == null)
        {
            _logger.LogInformation("RescueCompletionAppService - ValidateCreateInput: No accepted initiation found for Request: {RequestId}", input.RequestRescueId);
            throw new UserFriendlyException("No rescuer has been accepted for this request.", "400");
        }

        if (acceptedInitiation.RescuerId != _currentUser.Id.Value)
        {
            _logger.LogInformation("RescueCompletionAppService - ValidateCreateInput: User {UserId} is not the selected rescuer for Request: {RequestId}", _currentUser.Id.Value, input.RequestRescueId);
            throw new UserFriendlyException("Only the selected rescuer can complete this request.", "403");
        }

        // Check if completion already exists
        var existingCompletion = await _rescueCompletionRepository.FindAsync(x => x.RequestRescueId == input.RequestRescueId);
        if (existingCompletion != null)
        {
            _logger.LogInformation("RescueCompletionAppService - ValidateCreateInput: Completion already exists for Request: {RequestId}", input.RequestRescueId);
            throw new UserFriendlyException("Completion already exists for this request.", "400");
        }
    }

    /// <summary>
    /// Retrieves the Rescue Completion by its id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    private async Task<RescueCompletion> GetRescueCompletionAsync(Guid id)
    {
        var rescueCompletion = await _rescueCompletionRepository.FindAsync(x => x.Id == id);
        if (rescueCompletion is null)
        {
            _logger.LogInformation("RescueCompletionAppService - GetRescueCompletionAsync: {Message} by User: {UserId}", ErrorConsts.NotFound, _currentUser.Id);
            throw new UserFriendlyException(ErrorConsts.NotFound, "404");
        }
        return rescueCompletion;
    }

    /// <summary>
    /// Update RequestRescue status
    /// </summary>
    /// <param name="requestRescueId"></param>
    /// <param name="status"></param>
    /// <returns></returns>
    private async Task UpdateRequestRescueStatusAsync(Guid requestRescueId, string status)
    {
        var requestRescue = await _requestRescueRepository.GetAsync(requestRescueId);
        requestRescue.Status = status;
        await _requestRescueRepository.UpdateAsync(requestRescue);
        _logger.LogInformation("RescueCompletionAppService - UpdateRequestRescueStatusAsync: Updated Request Rescue status to {Status} for ID: {RequestId}", status, requestRescueId);
    }

    #endregion
}