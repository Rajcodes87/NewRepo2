using AnimalRescueSystem.Constants;
using AnimalRescueSystem.Dtos;
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
using Volo.Abp.Uow;
using Volo.Abp.Users;

namespace AnimalRescueSystem.RequestRescues;

[AllowAnonymous] // Allow public access to view and create rescue requests
public class RequestRescueAppService : ApplicationService, IRequestRescueAppService
{
    private readonly IRepository<RequestRescue, Guid> _requestRescueRepository;
    private readonly ILogger<RequestRescueAppService> _logger;
    private readonly ICurrentUser _currentUser;
    private readonly IMapper _mapper;

    public RequestRescueAppService(
        IRepository<RequestRescue, Guid> requestRescueRepository,
        ILogger<RequestRescueAppService> logger,
        ICurrentUser currentUser,
        IMapper mapper)
    {
        _requestRescueRepository = requestRescueRepository;
        _logger = logger;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    #region Public Methods

    /// <summary>
    /// Create a new Request Rescue
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="UserFriendlyException"></exception>
    //[Authorize(AnimalRescueSystemPermissions.RequestRescues.Create)]
    public async Task<ResponseDataDto<RequestRescueResponseDto>> CreateAsync(CreateUpdateRequestRescueDto input)
    {
        try
        {
            _logger.LogInformation("RequestRescueAppService - CreateAsync: Started by User: {UserId}", _currentUser?.Id);

            await ValidateInput(input);

            var requestRescue = _mapper.Map<CreateUpdateRequestRescueDto, RequestRescue>(input);

            await _requestRescueRepository.InsertAsync(requestRescue);

            var requestRescueDto = _mapper.Map<RequestRescue, RequestRescueDto>(requestRescue);
            _logger.LogInformation("RequestRescueAppService - CreateAsync: {Message} by User: {UserId}", CommonMessageConsts.DataSavedSuccessfully, _currentUser?.Id);

            var response = new ResponseDataDto<RequestRescueResponseDto>
            {
                Code = 200,
                Success = true,
                Message = CommonMessageConsts.DataSavedSuccessfully,
                Data = new RequestRescueResponseDto
                {
                    Id = requestRescueDto.Id
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
            _logger.LogError(ex, "RequestRescueAppService - CreateAsync: Requested by User: {UserId}, Error: {ErrorMessage}", _currentUser?.Id, ErrorConsts.ServerError);
            throw new UserFriendlyException(ErrorConsts.ServerError, "500");
        }
    }

    /// <summary>
    /// Update an existing Request Rescue
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="UserFriendlyException"></exception>
    //[Authorize(AnimalRescueSystemPermissions.RequestRescues.Update)]
    public async Task<ResponseDataDto<RequestRescueResponseDto>> UpdateAsync([Required(ErrorMessage = "Id is required.")] Guid id, CreateUpdateRequestRescueDto input)
    {
        try
        {
            _logger.LogInformation("RequestRescueAppService - UpdateAsync: Started by User: {UserId}", _currentUser?.Id);
            var requestRescue = await GetRequestRescueAsync(id);
            await ValidateInput(input, id);

            // Don't map Status - it's managed by workflow
            var currentStatus = requestRescue.Status;
            _mapper.Map(input, requestRescue);
            requestRescue.Status = currentStatus;

            await _requestRescueRepository.UpdateAsync(requestRescue);

            var requestRescueDto = _mapper.Map<RequestRescueDto>(requestRescue);

            _logger.LogInformation("RequestRescueAppService - UpdateAsync: {Message} by User: {UserId}", CommonMessageConsts.DataSavedSuccessfully, _currentUser?.Id);

            var response = new ResponseDataDto<RequestRescueResponseDto>
            {
                Code = 200,
                Success = true,
                Message = CommonMessageConsts.DataSavedSuccessfully,
                Data = new RequestRescueResponseDto
                {
                    Id = requestRescueDto.Id
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
            _logger.LogError(ex, "RequestRescueAppService - UpdateAsync: Requested by User: {UserId}, Error: {ErrorMessage}", _currentUser?.Id, ErrorConsts.ServerError);
            throw new UserFriendlyException(ErrorConsts.ServerError, "500");
        }
    }

    /// <summary>
    /// Get a list of Request Rescues
    /// </summary>
    /// <param name="input"></param>
    /// <param name="filter"></param>
    /// <returns></returns>
    /// <exception cref="UserFriendlyException"></exception>
    //[Authorize(AnimalRescueSystemPermissions.RequestRescues.Default)]
    public async Task<ResponseDataDto<PagedResultDto<RequestRescueDto>>> GetListAsync(PagedAndSortedResultRequestDto input, RequestRescueFilter filter)
    {
        try
        {
            _logger.LogInformation("RequestRescueAppService - GetListAsync: Started by User: {UserId}", _currentUser?.Id);

            if (input.Sorting.IsNullOrWhiteSpace())
            {
                input.Sorting = "RequestDate DESC";
            }

            filter.SearchKeyword = filter.SearchKeyword?.Trim()?.ToLower();

            var requestRescues = await _requestRescueRepository.WithDetailsAsync(x => x.RescueInitiations, x => x.RescueCompletion);

            var query = (from rr in requestRescues
                         select new RequestRescueDto
                         {
                             Id = rr.Id,
                             Title = rr.Title,
                             Location = rr.Location,
                             Description = rr.Description,
                             Picture = rr.Picture,
                             ContactNo = rr.ContactNo,
                             ContactName = rr.ContactName,
                             RequestDate = rr.RequestDate,
                             Status = rr.Status,
                             IsActive = rr.IsActive,
                             CreationTime = rr.CreationTime,
                             InitiationsCount = rr.RescueInitiations.Count,
                             HasCompletion = rr.RescueCompletion != null
                         })
                        .WhereIf(!string.IsNullOrWhiteSpace(filter.SearchKeyword), x =>
                                                        x.Title.ToLower().Contains(filter.SearchKeyword) ||
                                                        x.Location.ToLower().Contains(filter.SearchKeyword) ||
                                                        (x.Description != null && x.Description.ToLower().Contains(filter.SearchKeyword))
                                                        )
                        .WhereIf(!string.IsNullOrWhiteSpace(filter.Status), x => x.Status == filter.Status)
                        .WhereIf(filter.IsActive.HasValue, x => x.IsActive == filter.IsActive.Value);

            var dtos = await AsyncExecuter.ToListAsync(query
                .OrderBy(input.Sorting)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount));

            var totalCount = await query.CountAsync();

            _logger.LogInformation("RequestRescueAppService - GetListAsync: {Message} by User: {UserId}", CommonMessageConsts.DataRetrievedSuccessfully, _currentUser?.Id);

            var result = new PagedResultDto<RequestRescueDto>(totalCount, dtos);
            return new ResponseDataDto<PagedResultDto<RequestRescueDto>>()
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
            _logger.LogError(ex, "RequestRescueAppService - GetListAsync: Requested by User: {UserId}, Error: {ErrorMessage}", _currentUser?.Id, ErrorConsts.ServerError);
            throw new UserFriendlyException(ErrorConsts.InternalServerError, "500");
        }
    }

    /// <summary>
    /// Get a Request Rescue by its id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="UserFriendlyException"></exception>
    //[Authorize(AnimalRescueSystemPermissions.RequestRescues.Default)]
    public async Task<ResponseDataDto<RequestRescueDto>> GetAsync([Required(ErrorMessage = "Id is required.")] Guid id)
    {
        try
        {
            _logger.LogInformation("RequestRescueAppService - GetAsync: Started by User: {UserId}", _currentUser.Id);

            var requestRescue = await _requestRescueRepository.WithDetailsAsync(x => x.RescueInitiations, x => x.RescueCompletion);
            var entity = requestRescue.FirstOrDefault(x => x.Id == id);

            if (entity == null)
            {
                _logger.LogInformation("RequestRescueAppService - GetAsync: {Message} by User: {UserId}", ErrorConsts.NotFound, _currentUser.Id);
                throw new UserFriendlyException(ErrorConsts.NotFound, "404");
            }

            var response = _mapper.Map<RequestRescue, RequestRescueDto>(entity);

            _logger.LogInformation("RequestRescueAppService - GetAsync: {Message} by User: {UserId}", CommonMessageConsts.DataRetrievedSuccessfully, _currentUser.Id);

            return new ResponseDataDto<RequestRescueDto>
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
            _logger.LogError(ex, "RequestRescueAppService - GetAsync: Requested by User: {UserId}, Error: {ErrorMessage}", _currentUser.Id, ErrorConsts.ServerError);
            throw new UserFriendlyException(ErrorConsts.ServerError, "500");
        }
    }

    /// <summary>
    /// Delete a Request Rescue by its id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="UserFriendlyException"></exception>
    ///[Authorize(AnimalRescueSystemPermissions.RequestRescues.Delete)]
    public async Task<ResponseDataDto<RequestRescueResponseDto>> DeleteAsync([Required(ErrorMessage = "Id is required.")] Guid id)
    {
        try
        {
            _logger.LogInformation("RequestRescueAppService - DeleteAsync: Started by User: {UserId}", _currentUser.Id);

            var requestRescue = await GetRequestRescueAsync(id);

            // Cannot delete if there are initiations or completions
            if (requestRescue.RescueInitiations?.Any() == true)
            {
                _logger.LogWarning("RequestRescueAppService - DeleteAsync: Cannot delete request with initiations: {RequestId}", id);
                throw new UserFriendlyException("Cannot delete a request that has rescue initiations.", "400");
            }

            await _requestRescueRepository.DeleteAsync(id);
            _logger.LogInformation("RequestRescueAppService - DeleteAsync: {message} by User: {UserId}", CommonMessageConsts.DataDeletedSuccessfully, _currentUser.Id);

            var response = new ResponseDataDto<RequestRescueResponseDto>
            {
                Code = 200,
                Success = true,
                Message = CommonMessageConsts.DataDeletedSuccessfully,
                Data = new RequestRescueResponseDto
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
            _logger.LogError(ex, "RequestRescueAppService - DeleteAsync: Requested by User: {UserId}, Error: {ErrorMessage}", _currentUser.Id, ErrorConsts.ServerError);
            throw new UserFriendlyException(ErrorConsts.ServerError, "500");
        }
    }

    /// <summary>
    /// Activates a Request Rescue by its ID.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="UserFriendlyException"></exception>
    //[Authorize(AnimalRescueSystemPermissions.RequestRescues.Activate)]
    public async Task<ResponseDataDto<object>> ActivateAsync([Required(ErrorMessage = "Id is required.")] Guid id)
    {
        try
        {
            _logger.LogInformation("RequestRescueAppService - ActivateAsync: Activate Request Rescue requested for ID: {RequestRescue} by User: {UserId}", id, _currentUser?.Id);

            using var uow = UnitOfWorkManager.Begin();

            var requestRescue = await _requestRescueRepository.FindAsync(id) ?? throw new UserFriendlyException("Request Rescue not found.", "404");

            if (requestRescue.IsActive)
            {
                _logger.LogWarning("RequestRescueAppService - ActivateAsync: Attempted to activate an already active Request Rescue with ID: {RequestRescue} by User: {UserId}", id, _currentUser?.Id);
                throw new UserFriendlyException("Request Rescue is already active.", "400");
            }

            requestRescue.IsActive = true;
            await _requestRescueRepository.UpdateAsync(requestRescue);
            await uow.CompleteAsync();

            _logger.LogInformation("RequestRescueAppService - ActivateAsync: Request Rescue activated successfully for ID: {RequestRescue}", id);

            return new ResponseDataDto<object>
            {
                Code = 200,
                Success = true,
                Message = "Request Rescue activated successfully.",
            };
        }
        catch (UserFriendlyException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RequestRescueAppService - ActivateAsync: Error occurred while activating Request Rescue with ID: {RequestRescue}", id);
            throw new UserFriendlyException(ErrorConsts.ServerError, "500");
        }
    }

    /// <summary>
    /// Deactivates a Request Rescue by its ID.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="UserFriendlyException"></exception>
    //[Authorize(AnimalRescueSystemPermissions.RequestRescues.Deactivate)]
    public async Task<ResponseDataDto<object>> DeactivateAsync([Required(ErrorMessage = "Id is required.")] Guid id)
    {
        try
        {
            _logger.LogInformation("RequestRescueAppService - DeactivateAsync: Deactivate Request Rescue requested for ID: {RequestRescue} by User: {UserId}", id, _currentUser?.Id);

            using var uow = UnitOfWorkManager.Begin();

            var requestRescue = await _requestRescueRepository.FindAsync(id) ?? throw new UserFriendlyException("Request Rescue not found.", "404");

            if (!requestRescue.IsActive)
            {
                _logger.LogWarning("RequestRescueAppService - DeactivateAsync: Attempted to deactivate an already inactive Request Rescue with ID: {RequestRescue} by User: {UserId}", id, _currentUser?.Id);
                throw new UserFriendlyException("Request Rescue is already inactive.", "400");
            }

            requestRescue.IsActive = false;
            await _requestRescueRepository.UpdateAsync(requestRescue);
            await uow.CompleteAsync();

            _logger.LogInformation("RequestRescueAppService - DeactivateAsync: Request Rescue deactivated successfully for ID: {RequestRescue}", id);

            return new ResponseDataDto<object>
            {
                Code = 200,
                Success = true,
                Message = "Request Rescue deactivated successfully.",
            };
        }
        catch (UserFriendlyException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RequestRescueAppService - DeactivateAsync: Error occurred while deactivating Request Rescue with ID: {RequestRescue}", id);
            throw new UserFriendlyException(ErrorConsts.ServerError, "500");
        }
    }
    #endregion

    #region Private Method
    /// <summary>
    /// Validate the input for creating and updating a Request Rescue
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private async Task ValidateInput(CreateUpdateRequestRescueDto input, Guid? id = null)
    {
        input.Title = input.Title?.Trim();
        input.Location = input.Location?.Trim();

        if (await _requestRescueRepository.AnyAsync(x => x.Title.ToLower() == input.Title.ToLower() && x.Id != id))
        {
            _logger.LogInformation("RequestRescueAppService - ValidateInput: {Message} by User: {userId}", ErrorConsts.UniqueTitle, _currentUser?.Id);
            throw new UserFriendlyException(ErrorConsts.UniqueTitle, "400");
        }
    }

    /// <summary>
    /// Retrieves the Request Rescue by its id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    private async Task<RequestRescue> GetRequestRescueAsync(Guid id)
    {
        var requestRescues = await _requestRescueRepository.WithDetailsAsync(x => x.RescueInitiations, x => x.RescueCompletion);
        var requestRescue = requestRescues.FirstOrDefault(x => x.Id == id);

        if (requestRescue is null)
        {
            _logger.LogInformation("RequestRescueAppService - GetRequestRescueAsync: {Message} by User: {UserId}", ErrorConsts.NotFound, _currentUser.Id);
            throw new UserFriendlyException(ErrorConsts.NotFound, "404");
        }
        return requestRescue;
    }
    #endregion
}