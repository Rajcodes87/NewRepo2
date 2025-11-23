using AnimalRescueSystem.Constants;
using AnimalRescueSystem.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Pawchums.Identity;
using Pawchums.Services;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Identity;

namespace Pawchums.Identity;

[AllowAnonymous] // Allow public access for registration
public class RescuerRegistrationAppService : ApplicationService, IRescuerRegistrationAppService
{
    private readonly IIdentityUserRepository _userRepository;
    private readonly IdentityUserManager _userManager;
    private readonly IIdentityRoleRepository _roleRepository;
    private readonly EmailVerificationService _emailVerificationService;
    private readonly ILogger<RescuerRegistrationAppService> _logger;

    public RescuerRegistrationAppService(
        IIdentityUserRepository userRepository,
        IdentityUserManager userManager,
        IIdentityRoleRepository roleRepository,
        EmailVerificationService emailVerificationService,
        ILogger<RescuerRegistrationAppService> logger)
    {
        _userRepository = userRepository;
        _userManager = userManager;
        _roleRepository = roleRepository;
        _emailVerificationService = emailVerificationService;
        _logger = logger;
    }

    public async Task<ResponseDataDto<RescuerRegistrationResponseDto>> RegisterAsync(RegisterRescuerDto input)
    {
        try
        {
            _logger.LogInformation("RescuerRegistrationAppService - RegisterAsync: Registration attempt for {Email}", input.Email);

            // Check if username already exists
            var existingUserByUserName = await _userRepository.FindByNormalizedUserNameAsync(
                _userManager.NormalizeName(input.UserName));
            if (existingUserByUserName != null)
            {
                _logger.LogWarning("RescuerRegistrationAppService - RegisterAsync: Username {UserName} already exists", input.UserName);
                throw new UserFriendlyException("Username is already taken.", "400");
            }

            // Check if email already exists
            var existingUserByEmail = await _userRepository.FindByNormalizedEmailAsync(
                _userManager.NormalizeEmail(input.Email));
            if (existingUserByEmail != null)
            {
                _logger.LogWarning("RescuerRegistrationAppService - RegisterAsync: Email {Email} already exists", input.Email);
                throw new UserFriendlyException("Email is already registered.", "400");
            }

            // Create new user
            var newUser = new IdentityUser(
                id: GuidGenerator.Create(),
                userName: input.UserName,
                email: input.Email,
                tenantId: CurrentTenant.Id
            )
            {
                Name = input.Name,
                Surname = input.Surname
            };

            newUser.SetEmailConfirmed(false);
            newUser.SetIsActive(false); // Account inactive until email verification

            // Create user with password
            var result = await _userManager.CreateAsync(newUser, input.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors);
                _logger.LogError("RescuerRegistrationAppService - RegisterAsync: User creation failed: {Errors}", errors);
                throw new UserFriendlyException($"User registration failed: {errors}", "400");
            }

            // Assign Rescuer role
            var rescuerRole = await _roleRepository.FindByNormalizedNameAsync("RESCUER");
            if (rescuerRole == null)
            {
                _logger.LogError("RescuerRegistrationAppService - RegisterAsync: Rescuer role not found");
                throw new UserFriendlyException("Rescuer role is not configured. Please contact the administrator.", "500");
            }

            await _userManager.AddToRoleAsync(newUser, rescuerRole.Name);

            // Send verification email
            await _emailVerificationService.GenerateAndSendVerificationCodeAsync(
                newUser.Email,
                "Registration",
                newUser.Id);

            _logger.LogInformation("RescuerRegistrationAppService - RegisterAsync: User {Email} registered successfully. Verification email sent.", input.Email);

            var response = new ResponseDataDto<RescuerRegistrationResponseDto>
            {
                Code = 200,
                Success = true,
                Message = "Registration successful! A verification code has been sent to your email. Please check your inbox.",
                Data = new RescuerRegistrationResponseDto
                {
                    UserId = newUser.Id,
                    UserName = newUser.UserName,
                    Email = newUser.Email
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
            _logger.LogError(ex, "RescuerRegistrationAppService - RegisterAsync: Unexpected error during registration for {Email}", input.Email);
            throw new UserFriendlyException("An error occurred during registration. Please try again later.", "500");
        }
    }

    public async Task<ResponseDataDto<object>> VerifyEmailAsync(VerifyEmailDto input)
    {
        try
        {
            _logger.LogInformation("RescuerRegistrationAppService - VerifyEmailAsync: Verifying email {Email}", input.Email);

            // Verify the code
            var isValid = await _emailVerificationService.VerifyCodeAsync(input.Email, input.Code, "Registration");

            if (!isValid)
            {
                throw new UserFriendlyException("Invalid or expired verification code.", "400");
            }

            // Find the user and activate
            var user = await _userRepository.FindByNormalizedEmailAsync(_userManager.NormalizeEmail(input.Email));
            if (user == null)
            {
                throw new UserFriendlyException("User not found.", "404");
            }

            // Activate user
            user.SetIsActive(true);
            user.SetEmailConfirmed(true);
            await _userRepository.UpdateAsync(user);

            _logger.LogInformation("RescuerRegistrationAppService - VerifyEmailAsync: Email verified and account activated for {Email}", input.Email);

            return new ResponseDataDto<object>
            {
                Code = 200,
                Success = true,
                Message = "Email verified successfully! Your account is now active. You can log in now."
            };
        }
        catch (UserFriendlyException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RescuerRegistrationAppService - VerifyEmailAsync: Error verifying email {Email}", input.Email);
            throw new UserFriendlyException("An error occurred during verification. Please try again later.", "500");
        }
    }

    public async Task<ResponseDataDto<object>> ForgotPasswordAsync(ForgotPasswordDto input)
    {
        try
        {
            _logger.LogInformation("RescuerRegistrationAppService - ForgotPasswordAsync: Password reset requested for {Email}", input.Email);

            // Check if user exists
            var user = await _userRepository.FindByNormalizedEmailAsync(_userManager.NormalizeEmail(input.Email));
            if (user == null)
            {
                // Don't reveal if user exists or not
                _logger.LogWarning("RescuerRegistrationAppService - ForgotPasswordAsync: User not found for {Email}", input.Email);
                return new ResponseDataDto<object>
                {
                    Code = 200,
                    Success = true,
                    Message = "If an account exists with this email, a password reset code has been sent."
                };
            }

            // Send verification code
            await _emailVerificationService.GenerateAndSendVerificationCodeAsync(
                user.Email,
                "PasswordReset",
                user.Id);

            _logger.LogInformation("RescuerRegistrationAppService - ForgotPasswordAsync: Password reset code sent to {Email}", input.Email);

            return new ResponseDataDto<object>
            {
                Code = 200,
                Success = true,
                Message = "A password reset code has been sent to your email. Please check your inbox."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RescuerRegistrationAppService - ForgotPasswordAsync: Error processing password reset for {Email}", input.Email);
            throw new UserFriendlyException("An error occurred. Please try again later.", "500");
        }
    }

    public async Task<ResponseDataDto<object>> ResetPasswordAsync(ResetPasswordDto input)
    {
        try
        {
            _logger.LogInformation("RescuerRegistrationAppService - ResetPasswordAsync: Password reset attempt for {Email}", input.Email);

            // Verify the code
            var isValid = await _emailVerificationService.VerifyCodeAsync(input.Email, input.Code, "PasswordReset");

            if (!isValid)
            {
                throw new UserFriendlyException("Invalid or expired verification code.", "400");
            }

            // Find the user
            var user = await _userRepository.FindByNormalizedEmailAsync(_userManager.NormalizeEmail(input.Email));
            if (user == null)
            {
                throw new UserFriendlyException("User not found.", "404");
            }

            // Reset password
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, input.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors);
                _logger.LogError("RescuerRegistrationAppService - ResetPasswordAsync: Password reset failed: {Errors}", errors);
                throw new UserFriendlyException($"Password reset failed: {errors}", "400");
            }

            _logger.LogInformation("RescuerRegistrationAppService - ResetPasswordAsync: Password reset successful for {Email}", input.Email);

            return new ResponseDataDto<object>
            {
                Code = 200,
                Success = true,
                Message = "Password reset successfully! You can now log in with your new password."
            };
        }
        catch (UserFriendlyException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RescuerRegistrationAppService - ResetPasswordAsync: Error resetting password for {Email}", input.Email);
            throw new UserFriendlyException("An error occurred during password reset. Please try again later.", "500");
        }
    }
}
