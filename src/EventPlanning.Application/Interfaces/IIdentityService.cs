using EventPlanning.Application.DTOs.Auth;

namespace EventPlanning.Application.Interfaces;

public interface IIdentityService
{
    Task<(bool Succeeded, string[] Errors)> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
    Task<(bool Succeeded, string[] Errors)> UpdatePhoneNumberAsync(Guid userId, string phoneNumber);
    Task<(bool Succeeded, string[] Errors, Guid? UserId, string? Code)> RegisterUserAsync(RegisterUserDto model);
}