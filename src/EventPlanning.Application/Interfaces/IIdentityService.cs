using EventPlanning.Domain.Entities;

namespace EventPlanning.Application.Interfaces;

public interface IIdentityService
{
    Task<User?> GetUserByIdAsync(string userId);
    Task<(bool Succeeded, string[] Errors)> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
    Task<(bool Succeeded, string[] Errors)> UpdatePhoneNumberAsync(string userId, string phoneNumber);
}