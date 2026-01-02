using EventPlanning.Application.Interfaces;
using EventPlanning.Domain.Entities;
using EventPlanning.Domain.Interfaces;
using EventPlanning.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace EventPlanning.Infrastructure.Services;

public class IdentityService(
    UserManager<ApplicationUser> userManager,
    IUserRepository userRepository) : IIdentityService
{
    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        var appUser = await userManager.FindByIdAsync(userId.ToString());
        if (appUser == null) return null;

        return await userRepository.GetByIdAsync(userId, default);
    }

    public async Task<(bool Succeeded, string[] Errors)> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        var appUser = await userManager.FindByIdAsync(userId.ToString());
        if (appUser == null) return (false, ["User not found"]);

        var result = await userManager.ChangePasswordAsync(appUser, currentPassword, newPassword);
        return (result.Succeeded, result.Errors.Select(e => e.Description).ToArray());
    }

    public async Task<(bool Succeeded, string[] Errors)> UpdatePhoneNumberAsync(Guid userId, string phoneNumber)
    {
        var appUser = await userManager.FindByIdAsync(userId.ToString());
        if (appUser == null) return (false, ["User not found"]);

        var token = await userManager.GenerateChangePhoneNumberTokenAsync(appUser, phoneNumber);
        var result = await userManager.ChangePhoneNumberAsync(appUser, phoneNumber, token);
        return (result.Succeeded, result.Errors.Select(e => e.Description).ToArray());
    }
}