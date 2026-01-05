using EventPlanning.Application.Interfaces;
using EventPlanning.Domain.Entities;
using EventPlanning.Domain.Interfaces;
using EventPlanning.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using EventPlanning.Application.DTOs.Auth;
using EventPlanning.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EventPlanning.Infrastructure.Services;

public class IdentityService(
    UserManager<ApplicationUser> userManager,
    IUserRepository userRepository,
    Persistence.ApplicationDbContext context) : IIdentityService
{

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

    public async Task<(bool Succeeded, string[] Errors, Guid? UserId, string? Code)> RegisterUserAsync(RegisterUserDto model)
    {
        var strategy = context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var appUser = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber
                };

                var result = await userManager.CreateAsync(appUser, model.Password);
                if (!result.Succeeded)
                {
                    return (false, result.Errors.Select(e => e.Description).ToArray(), null, (string?)null);
                }

                var domainUser = new User(
                    appUser.Id.ToString(),
                    model.FirstName,
                    model.LastName,
                    UserRole.User,
                    model.Email,
                    model.Email,
                    model.PhoneNumber,
                    model.CountryCode
                );

                await userRepository.AddAsync(domainUser);

                var userId = await userManager.GetUserIdAsync(appUser);
                var code = await userManager.GenerateEmailConfirmationTokenAsync(appUser);

                await transaction.CommitAsync();

                return (true, Array.Empty<string>(), (Guid?)Guid.Parse(userId), code);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }
}