using EventPlanning.Application.Interfaces;
using EventPlanning.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace EventPlanning.Infrastructure.Services;

public class IdentityEmailSender(IEmailService emailService) : IEmailSender<User>
{
    public async Task SendConfirmationLinkAsync(User user, string email, string confirmationLink)
    {
        await emailService.SendEmailAsync(
            email,
            "Confirm your email",
            $"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.");
    }

    public async Task SendPasswordResetLinkAsync(User user, string email, string resetLink)
    {
        await emailService.SendEmailAsync(
            email,
            "Reset your password",
            $"Please reset your password by <a href='{resetLink}'>clicking here</a>.");
    }

    public async Task SendPasswordResetCodeAsync(User user, string email, string resetCode)
    {
        await emailService.SendEmailAsync(
            email,
            "Reset your password",
            $"Please reset your password using the following code: {resetCode}");
    }
}
