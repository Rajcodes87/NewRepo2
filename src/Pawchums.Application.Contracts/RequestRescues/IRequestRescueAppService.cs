using AnimalRescueSystem.Dtos;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace AnimalRescueSystem.RequestRescues;

public interface IRequestRescueAppService : IApplicationService
{
    Task<ResponseDataDto<RequestRescueResponseDto>> CreateAsync(CreateUpdateRequestRescueDto input);
    Task<ResponseDataDto<RequestRescueResponseDto>> UpdateAsync([Required(ErrorMessage = "Id is required.")] Guid id, CreateUpdateRequestRescueDto input);
    Task<ResponseDataDto<PagedResultDto<RequestRescueDto>>> GetListAsync(PagedAndSortedResultRequestDto input, RequestRescueFilter filter);
    Task<ResponseDataDto<RequestRescueDto>> GetAsync([Required(ErrorMessage = "Id is required.")] Guid id);
    Task<ResponseDataDto<RequestRescueResponseDto>> DeleteAsync([Required(ErrorMessage = "Id is required.")] Guid id);
    Task<ResponseDataDto<object>> ActivateAsync([Required(ErrorMessage = "Id is required.")] Guid id);
    Task<ResponseDataDto<object>> DeactivateAsync([Required(ErrorMessage = "Id is required.")] Guid id);
}
