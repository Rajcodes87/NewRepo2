using AnimalRescueSystem.Dtos;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace AnimalRescueSystem.RescuerApplications;

public interface IRescuerApplicationAppService : IApplicationService
{
    Task<ResponseDataDto<RescuerApplicationDto>> CreateAsync(CreateRescuerApplicationDto input);
    Task<ResponseDataDto<PagedResultDto<RescuerApplicationDto>>> GetListAsync(PagedAndSortedResultRequestDto input, RescuerApplicationFilter filter);
    Task<ResponseDataDto<RescuerApplicationDto>> GetAsync(Guid id);
    Task<ResponseDataDto<object>> ReviewAsync(ReviewRescuerApplicationDto input);
    Task<ResponseDataDto<RescuerApplicationDto>> GetByUserIdAsync(Guid userId);
}