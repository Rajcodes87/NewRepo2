using AnimalRescueSystem.Dtos;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace AnimalRescueSystem.RescueInitiations;

public interface IRescueInitiationAppService : IApplicationService
{
    Task<ResponseDataDto<RescueInitiationResponseDto>> CreateAsync(CreateRescueInitiationDto input);
    Task<ResponseDataDto<RescueInitiationResponseDto>> UpdateAsync([Required(ErrorMessage = "Id is required.")] Guid id, UpdateRescueInitiationDto input);
    Task<ResponseDataDto<PagedResultDto<RescueInitiationDto>>> GetListAsync(PagedAndSortedResultRequestDto input, RescueInitiationFilter filter);
    Task<ResponseDataDto<RescueInitiationDto>> GetAsync([Required(ErrorMessage = "Id is required.")] Guid id);
    Task<ResponseDataDto<RescueInitiationResponseDto>> DeleteAsync([Required(ErrorMessage = "Id is required.")] Guid id);
    Task<ResponseDataDto<object>> AcceptInitiationAsync([Required(ErrorMessage = "Id is required.")] Guid id);
    Task<ResponseDataDto<object>> RejectInitiationAsync([Required(ErrorMessage = "Id is required.")] Guid id);
    Task<ResponseDataDto<object>> WithdrawInitiationAsync([Required(ErrorMessage = "Id is required.")] Guid id);
    Task<ResponseDataDto<PagedResultDto<RescueInitiationDto>>> GetInitiationsForRequestAsync([Required(ErrorMessage = "Request Id is required.")] Guid requestId, PagedAndSortedResultRequestDto input);
    Task<ResponseDataDto<PagedResultDto<RescueInitiationDto>>> GetMyInitiationsAsync(PagedAndSortedResultRequestDto input);
}