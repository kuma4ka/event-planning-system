using EventPlanning.Domain.Entities;

namespace EventPlanning.Application.Interfaces;

public interface IIdentityService
{
    Task<User?> GetUserByIdAsync(Guid userId);
    Task<(bool Succeeded, string[] Errors)> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
    Task<(bool Succeeded, string[] Errors)> UpdatePhoneNumberAsync(Guid userId, string phoneNumber);
}