using AnimalRescueSystem.Constants;
using AnimalRescueSystem.Dtos;
using AnimalRescueSystem.Entities.RequestRescues;
using AnimalRescueSystem.RescueInitiations;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pawchums.Entities.RequestRescues;
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

namespace AnimalRescueSystem.RescueInitiations;

[Authorize(Roles = "Rescuer,admin")] // Only Rescuers and admins can access
public class RescueInitiationAppService : ApplicationService, IRescueInitiationAppService
{
    private readonly IRepository<RescueInitiation, Guid> _rescueInitiationRepository;
    private readonly IRepository<RequestRescue, Guid> _requestRescueRepository;
    private readonly IRepository<IdentityUser, Guid> _userRepository;
    private readonly ILogger<RescueInitiationAppService> _logger;
    private readonly ICurrentUser _currentUser;
    private readonly IMapper _mapper;

    public RescueInitiationAppService(
        IRepository<RescueInitiation, Guid> rescueInitiationRepository,
        IRepository<RequestRescue, Guid> requestRescueRepository,
        IRepository<IdentityUser, Guid> userRepository,
        ILogger<RescueInitiationAppService> logger,
        ICurrentUser currentUser,
        IMapper mapper)
    {
        _rescueInitiationRepository = rescueInitiationRepository;
        _requestRescueRepository = requestRescueRepository;
        _userRepository = userRepository;
        _logger = logger;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    #region Public Methods

    /// <summary>
    /// Create a new Rescue Initiation
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="UserFriendlyException"></exception>
    //[Authorize(AnimalRescueSystemPermissions.RescueInitiations.Create)]
    public async Task<ResponseDataDto<RescueInitiationResponseDto>> CreateAsync(CreateRescueInitiationDto input)
    {
        try
        {
            _logger.LogInformation("RescueInitiationAppService - CreateAsync: Started by User: {UserId}", _currentUser?.Id);

            await ValidateCreateInput(input);

            var rescueInitiation = _mapper.Map<CreateRescueInitiationDto, RescueInitiation>(input);
            rescueInitiation.RescuerId = _currentUser.Id.Value;

            await _rescueInitiationRepository.InsertAsync(rescueInitiation);

            // Update RequestRescue status to Initiated if it's NotInitiated
            await UpdateRequestRescueStatusAsync(input.RequestRescueId);

            var rescueInitiationDto = _mapper.Map<RescueInitiation, RescueInitiationDto>(rescueInitiation);
            _logger.LogInformation("RescueInitiationAppService - CreateAsync: {Message} by User: {UserId}", CommonMessageConsts.DataSavedSuccessfully, _currentUser?.Id);

            var response = new ResponseDataDto<RescueInitiationResponseDto>
            {
                Code = 200,
                Success = true,
                Message = CommonMessageConsts.DataSavedSuccessfully,
                Data = new RescueInitiationResponseDto
                {
                    Id = rescueInitiationDto.Id
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
            _logger.LogError(ex, "RescueInitiationAppService - CreateAsync: Requested by User: {UserId}, Error: {ErrorMessage}", _currentUser?.Id, ErrorConsts.ServerError);
            throw new UserFriendlyException(ErrorConsts.ServerError, "500");
        }
    }

    /// <summary>
    /// Update an existing Rescue Initiation
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="UserFriendlyException"></exception>
    //[Authorize(AnimalRescueSystemPermissions.RescueInitiations.Update)]
    public async Task<ResponseDataDto<RescueInitiationResponseDto>> UpdateAsync([Required(ErrorMessage = "Id is required.")] Guid id, UpdateRescueInitiationDto input)
    {
        try
        {
            _logger.LogInformation("RescueInitiationAppService - UpdateAsync: Started by User: {UserId}", _currentUser?.Id);

            var rescueInitiation = await GetRescueInitiationAsync(id);

            // Validate that only the rescuer can update their own initiation
            if (rescueInitiation.RescuerId != _currentUser.Id)
            {
                _logger.LogWarning("RescueInitiationAppService - UpdateAsync: Unauthorized update attempt by User: {UserId} for Initiation: {InitiationId}", _currentUser?.Id, id);
                throw new UserFriendlyException("You can only update your own initiations.", "403");
            }

            // Cannot update if already accepted or rejected
            if (rescueInitiation.Status == RescueInitiationConsts.InitiationStatus.Accepted ||
                rescueInitiation.Status == RescueInitiationConsts.InitiationStatus.Rejected)
            {
                _logger.LogWarning("RescueInitiationAppService - UpdateAsync: Attempt to update finalized initiation: {InitiationId}", id);
                throw new UserFriendlyException("Cannot update an initiation that has been accepted or rejected.", "400");
            }

            _mapper.Map(input, rescueInitiation);

            await _rescueInitiationRepository.UpdateAsync(rescueInitiation);

            var rescueInitiationDto = _mapper.Map<RescueInitiationDto>(rescueInitiation);

            _logger.LogInformation("RescueInitiationAppService - UpdateAsync: {Message} by User: {UserId}", CommonMessageConsts.DataSavedSuccessfully, _currentUser?.Id);

            var response = new ResponseDataDto<RescueInitiationResponseDto>
            {
                Code = 200,
                Success = true,
                Message = CommonMessageConsts.DataSavedSuccessfully,
                Data = new RescueInitiationResponseDto
                {
                    Id = rescueInitiationDto.Id
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
            _logger.LogError(ex, "RescueInitiationAppService - UpdateAsync: Requested by User: {UserId}, Error: {ErrorMessage}", _currentUser?.Id, ErrorConsts.ServerError);
            throw new UserFriendlyException(ErrorConsts.ServerError, "500");
        }
    }

    /// <summary>
    /// Get a list of Rescue Initiations
    /// </summary>
    /// <param name="input"></param>
    /// <param name="filter"></param>
    /// <returns></returns>
    /// <exception cref="UserFriendlyException"></exception>
    //[Authorize(AnimalRescueSystemPermissions.RescueInitiations.Default)]
    public async Task<ResponseDataDto<PagedResultDto<RescueInitiationDto>>> GetListAsync(PagedAndSortedResultRequestDto input, RescueInitiationFilter filter)
    {
        try
        {
            _logger.LogInformation("RescueInitiationAppService - GetListAsync: Started by User: {UserId}", _currentUser?.Id);

            if (input.Sorting.IsNullOrWhiteSpace())
            {
                input.Sorting = "InitiatedDate DESC";
            }

            filter.SearchKeyword = filter.SearchKeyword?.Trim()?.ToLower();

            var rescueInitiations = await _rescueInitiationRepository.GetQueryableAsync();
            var requestRescues = await _requestRescueRepository.GetQueryableAsync();
            var users = await _userRepository.GetQueryableAsync();

            var query = (from ri in rescueInitiations
                         join rr in requestRescues on ri.RequestRescueId equals rr.Id
                         join u in users on ri.RescuerId equals u.Id
                         join acceptedBy in users on ri.AcceptedByUserId equals acceptedBy.Id into acceptedByGroup
                         from acceptedBy in acceptedByGroup.DefaultIfEmpty()
                         select new RescueInitiationDto
                         {
                             Id = ri.Id,
                             RequestRescueId = ri.RequestRescueId,
                             RequestTitle = rr.Title,
                             RequestLocation = rr.Location,
                             RescuerId = ri.RescuerId,
                             RescuerName = u.Name,
                             RescuerEmail = u.Email,
                             InitiatedDate = ri.InitiatedDate,
                             Notes = ri.Notes,
                             Status = ri.Status,
                             IsSelected = ri.IsSelected,
                             AcceptedDate = ri.AcceptedDate,
                             AcceptedByUserId = ri.AcceptedByUserId,
                             AcceptedByUserName = acceptedBy != null ? acceptedBy.Name : null,
                             CreationTime = ri.CreationTime
                         })
                        .WhereIf(!string.IsNullOrWhiteSpace(filter.SearchKeyword), x =>
                                                        x.RequestTitle.ToLower().Contains(filter.SearchKeyword) ||
                                                        x.RequestLocation.ToLower().Contains(filter.SearchKeyword) ||
                                                        (x.RescuerName != null && x.RescuerName.ToLower().Contains(filter.SearchKeyword))
                                                        )
                        .WhereIf(!string.IsNullOrWhiteSpace(filter.Status), x => x.Status == filter.Status)
                        .WhereIf(filter.IsSelected.HasValue, x => x.IsSelected == filter.IsSelected.Value)
                        .WhereIf(filter.RequestRescueId.HasValue, x => x.RequestRescueId == filter.RequestRescueId.Value)
                        .WhereIf(filter.RescuerId.HasValue, x => x.RescuerId == filter.RescuerId.Value);

            var dtos = await AsyncExecuter.ToListAsync(query
                .OrderBy(input.Sorting)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount));

            var totalCount = await query.CountAsync();

            _logger.LogInformation("RescueInitiationAppService - GetListAsync: {Message} by User: {UserId}", CommonMessageConsts.DataRetrievedSuccessfully, _currentUser?.Id);

            var result = new PagedResultDto<RescueInitiationDto>(totalCount, dtos);
            return new ResponseDataDto<PagedResultDto<RescueInitiationDto>>()
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
            _logger.LogError(ex, "RescueInitiationAppService - GetListAsync: Requested by User: {UserId}, Error: {ErrorMessage}", _currentUser?.Id, ErrorConsts.ServerError);
            throw new UserFriendlyException(ErrorConsts.InternalServerError, "500");
        }
    }

    /// <summary>
    /// Get a Rescue Initiation by its id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="UserFriendlyException"></exception>
    //[Authorize(AnimalRescueSystemPermissions.RescueInitiations.Default)]
    public async Task<ResponseDataDto<RescueInitiationDto>> GetAsync([Required(ErrorMessage = "Id is required.")] Guid id)
    {
        try
        {
            _logger.LogInformation("RescueInitiationAppService - GetAsync: Started by User: {UserId}", _currentUser.Id);

            var rescueInitiation = await GetRescueInitiationAsync(id);
            var response = _mapper.Map<RescueInitiation, RescueInitiationDto>(rescueInitiation);

            // Populate related data
            var requestRescue = await _requestRescueRepository.GetAsync(rescueInitiation.RequestRescueId);
            var rescuer = await _userRepository.GetAsync(rescueInitiation.RescuerId);

            response.RequestTitle = requestRescue.Title;
            response.RequestLocation = requestRescue.Location;
            response.RescuerName = rescuer.Name;
            response.RescuerEmail = rescuer.Email;

            if (rescueInitiation.AcceptedByUserId.HasValue)
            {
                var acceptedByUser = await _userRepository.GetAsync(rescueInitiation.AcceptedByUserId.Value);
                response.AcceptedByUserName = acceptedByUser.Name;
            }

            _logger.LogInformation("RescueInitiationAppService - GetAsync: {Message} by User: {UserId}", CommonMessageConsts.DataRetrievedSuccessfully, _currentUser.Id);

            return new ResponseDataDto<RescueInitiationDto>
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
            _logger.LogError(ex, "RescueInitiationAppService - GetAsync: Requested by User: {UserId}, Error: {ErrorMessage}", _currentUser.Id, ErrorConsts.ServerError);
            throw new UserFriendlyException(ErrorConsts.ServerError, "500");
        }
    }

    /// <summary>
    /// Delete a Rescue Initiation by its id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="UserFriendlyException"></exception>
    //[Authorize(AnimalRescueSystemPermissions.RescueInitiations.Delete)]
    public async Task<ResponseDataDto<RescueInitiationResponseDto>> DeleteAsync([Required(ErrorMessage = "Id is required.")] Guid id)
    {
        try
        {
            _logger.LogInformation("RescueInitiationAppService - DeleteAsync: Started by User: {UserId}", _currentUser.Id);

            var rescueInitiation = await GetRescueInitiationAsync(id);

            // Validate that only the rescuer can delete their own initiation
            if (rescueInitiation.RescuerId != _currentUser.Id)
            {
                _logger.LogWarning("RescueInitiationAppService - DeleteAsync: Unauthorized delete attempt by User: {UserId} for Initiation: {InitiationId}", _currentUser?.Id, id);
                throw new UserFriendlyException("You can only delete your own initiations.", "403");
            }

            // Cannot delete if already accepted
            if (rescueInitiation.Status == RescueInitiationConsts.InitiationStatus.Accepted || rescueInitiation.IsSelected)
            {
                _logger.LogWarning("RescueInitiationAppService - DeleteAsync: Attempt to delete accepted initiation: {InitiationId}", id);
                throw new UserFriendlyException("Cannot delete an accepted initiation.", "400");
            }

            await _rescueInitiationRepository.DeleteAsync(id);
            _logger.LogInformation("RescueInitiationAppService - DeleteAsync: {message} by User: {UserId}", CommonMessageConsts.DataDeletedSuccessfully, _currentUser.Id);

            var response = new ResponseDataDto<RescueInitiationResponseDto>
            {
                Code = 200,
                Success = true,
                Message = CommonMessageConsts.DataDeletedSuccessfully,
                Data = new RescueInitiationResponseDto
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
            _logger.LogError(ex, "RescueInitiationAppService - DeleteAsync: Requested by User: {UserId}, Error: {ErrorMessage}", _currentUser.Id, ErrorConsts.ServerError);
            throw new UserFriendlyException(ErrorConsts.ServerError, "500");
        }
    }

    /// <summary>
    /// Accept a Rescue Initiation (Admin operation)
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="UserFriendlyException"></exception>
    //[Authorize(AnimalRescueSystemPermissions.RescueInitiations.Accept)]
    public async Task<ResponseDataDto<object>> AcceptInitiationAsync([Required(ErrorMessage = "Id is required.")] Guid id)
    {
        try
        {
            _logger.LogInformation("RescueInitiationAppService - AcceptInitiationAsync: Accept Rescue Initiation requested for ID: {InitiationId} by User: {UserId}", id, _currentUser?.Id);

            using var uow = UnitOfWorkManager.Begin();

            var rescueInitiation = await _rescueInitiationRepository.FindAsync(id) ?? throw new UserFriendlyException("Rescue Initiation not found.", "404");

            if (rescueInitiation.Status != RescueInitiationConsts.InitiationStatus.Pending)
            {
                _logger.LogWarning("RescueInitiationAppService - AcceptInitiationAsync: Attempted to accept non-pending initiation with ID: {InitiationId}", id);
                throw new UserFriendlyException("Only pending initiations can be accepted.", "400");
            }

            // Reject all other pending initiations for this request
            var otherInitiations = await _rescueInitiationRepository.GetListAsync(x =>
                x.RequestRescueId == rescueInitiation.RequestRescueId &&
                x.Id != id &&
                x.Status == RescueInitiationConsts.InitiationStatus.Pending);

            foreach (var other in otherInitiations)
            {
                other.Status = RescueInitiationConsts.InitiationStatus.Rejected;
                await _rescueInitiationRepository.UpdateAsync(other);
            }

            // Accept this initiation
            rescueInitiation.Status = RescueInitiationConsts.InitiationStatus.Accepted;
            rescueInitiation.IsSelected = true;
            rescueInitiation.AcceptedDate = DateTime.UtcNow;
            rescueInitiation.AcceptedByUserId = _currentUser.Id;
            await _rescueInitiationRepository.UpdateAsync(rescueInitiation);

            // Update RequestRescue status to InProgress
            var requestRescue = await _requestRescueRepository.GetAsync(rescueInitiation.RequestRescueId);
            requestRescue.Status = RequestRescueConsts.RequestStatus.InProgress;
            await _requestRescueRepository.UpdateAsync(requestRescue);

            await uow.CompleteAsync();

            _logger.LogInformation("RescueInitiationAppService - AcceptInitiationAsync: Rescue Initiation accepted successfully for ID: {InitiationId}", id);

            return new ResponseDataDto<object>
            {
                Code = 200,
                Success = true,
                Message = "Rescue Initiation accepted successfully.",
            };
        }
        catch (UserFriendlyException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RescueInitiationAppService - AcceptInitiationAsync: Error occurred while accepting Rescue Initiation with ID: {InitiationId}", id);
            throw new UserFriendlyException(ErrorConsts.ServerError, "500");
        }
    }

    /// <summary>
    /// Reject a Rescue Initiation (Admin operation)
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="UserFriendlyException"></exception>
    //[Authorize(AnimalRescueSystemPermissions.RescueInitiations.Reject)]
    public async Task<ResponseDataDto<object>> RejectInitiationAsync([Required(ErrorMessage = "Id is required.")] Guid id)
    {
        try
        {
            _logger.LogInformation("RescueInitiationAppService - RejectInitiationAsync: Reject Rescue Initiation requested for ID: {InitiationId} by User: {UserId}", id, _currentUser?.Id);

            using var uow = UnitOfWorkManager.Begin();

            var rescueInitiation = await _rescueInitiationRepository.FindAsync(id) ?? throw new UserFriendlyException("Rescue Initiation not found.", "404");

            if (rescueInitiation.Status != RescueInitiationConsts.InitiationStatus.Pending)
            {
                _logger.LogWarning("RescueInitiationAppService - RejectInitiationAsync: Attempted to reject non-pending initiation with ID: {InitiationId}", id);
                throw new UserFriendlyException("Only pending initiations can be rejected.", "400");
            }

            rescueInitiation.Status = RescueInitiationConsts.InitiationStatus.Rejected;
            await _rescueInitiationRepository.UpdateAsync(rescueInitiation);
            await uow.CompleteAsync();

            _logger.LogInformation("RescueInitiationAppService - RejectInitiationAsync: Rescue Initiation rejected successfully for ID: {InitiationId}", id);

            return new ResponseDataDto<object>
            {
                Code = 200,
                Success = true,
                Message = "Rescue Initiation rejected successfully.",
            };
        }
        catch (UserFriendlyException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RescueInitiationAppService - RejectInitiationAsync: Error occurred while rejecting Rescue Initiation with ID: {InitiationId}", id);
            throw new UserFriendlyException(ErrorConsts.ServerError, "500");
        }
    }

    /// <summary>
    /// Withdraw a Rescue Initiation (Rescuer operation)
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="UserFriendlyException"></exception>
    //[Authorize(AnimalRescueSystemPermissions.RescueInitiations.Withdraw)]
    public async Task<ResponseDataDto<object>> WithdrawInitiationAsync([Required(ErrorMessage = "Id is required.")] Guid id)
    {
        try
        {
            _logger.LogInformation("RescueInitiationAppService - WithdrawInitiationAsync: Withdraw Rescue Initiation requested for ID: {InitiationId} by User: {UserId}", id, _currentUser?.Id);

            using var uow = UnitOfWorkManager.Begin();

            var rescueInitiation = await _rescueInitiationRepository.FindAsync(id) ?? throw new UserFriendlyException("Rescue Initiation not found.", "404");

            // Validate that only the rescuer can withdraw their own initiation
            if (rescueInitiation.RescuerId != _currentUser.Id)
            {
                _logger.LogWarning("RescueInitiationAppService - WithdrawInitiationAsync: Unauthorized withdraw attempt by User: {UserId} for Initiation: {InitiationId}", _currentUser?.Id, id);
                throw new UserFriendlyException("You can only withdraw your own initiations.", "403");
            }

            if (rescueInitiation.Status == RescueInitiationConsts.InitiationStatus.Accepted || rescueInitiation.IsSelected)
            {
                _logger.LogWarning("RescueInitiationAppService - WithdrawInitiationAsync: Attempted to withdraw accepted initiation with ID: {InitiationId}", id);
                throw new UserFriendlyException("Cannot withdraw an accepted initiation.", "400");
            }

            if (rescueInitiation.Status == RescueInitiationConsts.InitiationStatus.Withdrawn)
            {
                _logger.LogWarning("RescueInitiationAppService - WithdrawInitiationAsync: Attempted to withdraw already withdrawn initiation with ID: {InitiationId}", id);
                throw new UserFriendlyException("Initiation is already withdrawn.", "400");
            }

            rescueInitiation.Status = RescueInitiationConsts.InitiationStatus.Withdrawn;
            await _rescueInitiationRepository.UpdateAsync(rescueInitiation);
            await uow.CompleteAsync();

            _logger.LogInformation("RescueInitiationAppService - WithdrawInitiationAsync: Rescue Initiation withdrawn successfully for ID: {InitiationId}", id);

            return new ResponseDataDto<object>
            {
                Code = 200,
                Success = true,
                Message = "Rescue Initiation withdrawn successfully.",
            };
        }
        catch (UserFriendlyException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RescueInitiationAppService - WithdrawInitiationAsync: Error occurred while withdrawing Rescue Initiation with ID: {InitiationId}", id);
            throw new UserFriendlyException(ErrorConsts.ServerError, "500");
        }
    }

    /// <summary>
    /// Get all initiations for a specific request
    /// </summary>
    /// <param name="requestId"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="UserFriendlyException"></exception>
    //[Authorize(AnimalRescueSystemPermissions.RescueInitiations.Default)]
    public async Task<ResponseDataDto<PagedResultDto<RescueInitiationDto>>> GetInitiationsForRequestAsync([Required(ErrorMessage = "Request Id is required.")] Guid requestId, PagedAndSortedResultRequestDto input)
    {
        try
        {
            _logger.LogInformation("RescueInitiationAppService - GetInitiationsForRequestAsync: Started for Request: {RequestId} by User: {UserId}", requestId, _currentUser?.Id);

            var filter = new RescueInitiationFilter
            {
                RequestRescueId = requestId
            };

            return await GetListAsync(input, filter);
        }
        catch (UserFriendlyException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RescueInitiationAppService - GetInitiationsForRequestAsync: Requested by User: {UserId}, Error: {ErrorMessage}", _currentUser?.Id, ErrorConsts.ServerError);
            throw new UserFriendlyException(ErrorConsts.ServerError, "500");
        }
    }

    /// <summary>
    /// Get all initiations for the current user (rescuer)
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="UserFriendlyException"></exception>
    //[Authorize(AnimalRescueSystemPermissions.RescueInitiations.Default)]
    public async Task<ResponseDataDto<PagedResultDto<RescueInitiationDto>>> GetMyInitiationsAsync(PagedAndSortedResultRequestDto input)
    {
        try
        {
            _logger.LogInformation("RescueInitiationAppService - GetMyInitiationsAsync: Started by User: {UserId}", _currentUser?.Id);

            if (!_currentUser.Id.HasValue)
            {
                throw new UserFriendlyException("User must be authenticated.", "401");
            }

            var filter = new RescueInitiationFilter
            {
                RescuerId = _currentUser.Id.Value
            };

            return await GetListAsync(input, filter);
        }
        catch (UserFriendlyException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RescueInitiationAppService - GetMyInitiationsAsync: Requested by User: {UserId}, Error: {ErrorMessage}", _currentUser?.Id, ErrorConsts.ServerError);
            throw new UserFriendlyException(ErrorConsts.ServerError, "500");
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Validate the input for creating a Rescue Initiation
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private async Task ValidateCreateInput(CreateRescueInitiationDto input)
    {
        // Check if request exists
        var requestRescue = await _requestRescueRepository.FindAsync(input.RequestRescueId);
        if (requestRescue == null)
        {
            _logger.LogInformation("RescueInitiationAppService - ValidateCreateInput: Request Rescue not found: {RequestId}", input.RequestRescueId);
            throw new UserFriendlyException("Request Rescue not found.", "404");
        }

        // Check if request is active
        if (!requestRescue.IsActive)
        {
            _logger.LogInformation("RescueInitiationAppService - ValidateCreateInput: Request Rescue is inactive: {RequestId}", input.RequestRescueId);
            throw new UserFriendlyException("Cannot initiate rescue for inactive request.", "400");
        }

        // Check if request is already completed or cancelled
        if (requestRescue.Status == RequestRescueConsts.RequestStatus.Completed ||
            requestRescue.Status == RequestRescueConsts.RequestStatus.Cancelled)
        {
            _logger.LogInformation("RescueInitiationAppService - ValidateCreateInput: Request Rescue is {Status}: {RequestId}", requestRescue.Status, input.RequestRescueId);
            throw new UserFriendlyException($"Cannot initiate rescue for {requestRescue.Status.ToLower()} request.", "400");
        }

        // Check if user is authenticated
        if (!_currentUser.Id.HasValue)
        {
            _logger.LogInformation("RescueInitiationAppService - ValidateCreateInput: User not authenticated");
            throw new UserFriendlyException("User must be authenticated to initiate rescue.", "401");
        }

        // Check if rescuer already initiated for this request
        var existingInitiation = await _rescueInitiationRepository.FindAsync(x =>
            x.RequestRescueId == input.RequestRescueId &&
            x.RescuerId == _currentUser.Id.Value);

        if (existingInitiation != null)
        {
            _logger.LogInformation("RescueInitiationAppService - ValidateCreateInput: Rescuer already initiated: {RescuerId} for Request: {RequestId}", _currentUser.Id.Value, input.RequestRescueId);
            throw new UserFriendlyException("You have already initiated this rescue request.", "400");
        }
    }

    /// <summary>
    /// Retrieves the Rescue Initiation by its id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    private async Task<RescueInitiation> GetRescueInitiationAsync(Guid id)
    {
        var rescueInitiation = await _rescueInitiationRepository.FindAsync(x => x.Id == id);
        if (rescueInitiation is null)
        {
            _logger.LogInformation("RescueInitiationAppService - GetRescueInitiationAsync: {Message} by User: {UserId}", ErrorConsts.NotFound, _currentUser.Id);
            throw new UserFriendlyException(ErrorConsts.NotFound, "404");
        }
        return rescueInitiation;
    }

    /// <summary>
    /// Update RequestRescue status when first initiation is created
    /// </summary>
    /// <param name="requestRescueId"></param>
    /// <returns></returns>
    private async Task UpdateRequestRescueStatusAsync(Guid requestRescueId)
    {
        var requestRescue = await _requestRescueRepository.GetAsync(requestRescueId);

        if (requestRescue.Status == RequestRescueConsts.RequestStatus.NotInitiated)
        {
            requestRescue.Status = RequestRescueConsts.RequestStatus.Initiated;
            await _requestRescueRepository.UpdateAsync(requestRescue);
            _logger.LogInformation("RescueInitiationAppService - UpdateRequestRescueStatusAsync: Updated Request Rescue status to Initiated for ID: {RequestId}", requestRescueId);
        }
    }

    #endregion
}