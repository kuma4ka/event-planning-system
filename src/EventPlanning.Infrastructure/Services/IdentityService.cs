using EventPlanning.Application.Interfaces;
using EventPlanning.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace EventPlanning.Infrastructure.Services;

public class IdentityService(UserManager<User> userManager) : IIdentityService
{
    public async Task<User?> GetUserByIdAsync(string userId)
    {
        return await userManager.FindByIdAsync(userId);
    }
}