using AnimalRescueSystem.Dtos;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Pawchums.Identity;

public interface IRescuerRegistrationAppService : IApplicationService
{
    Task<ResponseDataDto<RescuerRegistrationResponseDto>> RegisterAsync(RegisterRescuerDto input);
    Task<ResponseDataDto<object>> VerifyEmailAsync(VerifyEmailDto input);
    Task<ResponseDataDto<object>> ForgotPasswordAsync(ForgotPasswordDto input);
    Task<ResponseDataDto<object>> ResetPasswordAsync(ResetPasswordDto input);
}
