using AnimalRescueSystem.Dtos;
using Pawchums.RescueCompletions;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace AnimalRescueSystem.RescueCompletions;

public interface IRescueCompletionAppService : IApplicationService
{
    Task<ResponseDataDto<RescueCompletionResponseDto>> CreateAsync(CreateRescueCompletionDto input);
    Task<ResponseDataDto<RescueCompletionResponseDto>> UpdateAsync([Required(ErrorMessage = "Id is required.")] Guid id, UpdateRescueCompletionDto input);
    Task<ResponseDataDto<PagedResultDto<RescueCompletionDto>>> GetListAsync(PagedAndSortedResultRequestDto input, RescueCompletionFilter filter);
    Task<ResponseDataDto<RescueCompletionDto>> GetAsync([Required(ErrorMessage = "Id is required.")] Guid id);
    Task<ResponseDataDto<RescueCompletionDto>> GetByRequestIdAsync([Required(ErrorMessage = "Request Id is required.")] Guid requestId);
    Task<ResponseDataDto<RescueCompletionResponseDto>> DeleteAsync([Required(ErrorMessage = "Id is required.")] Guid id);
    Task<ResponseDataDto<object>> VerifyCompletionAsync([Required(ErrorMessage = "Id is required.")] Guid id, VerifyCompletionDto input);
    Task<ResponseDataDto<object>> UnverifyCompletionAsync([Required(ErrorMessage = "Id is required.")] Guid id);
    Task<ResponseDataDto<PagedResultDto<RescueCompletionDto>>> GetMyCompletionsAsync(PagedAndSortedResultRequestDto input);
}