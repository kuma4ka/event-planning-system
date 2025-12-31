using EventPlanning.Application.Interfaces;
using EventPlanning.Domain.Entities;
using EventPlanning.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace EventPlanning.Application.Services;

public class NewsletterService(INewsletterRepository newsletterRepository, IEmailService emailService, IHttpContextAccessor httpContextAccessor) : INewsletterService
{
    public async Task SubscribeAsync(string email, CancellationToken cancellationToken = default)
    {
        // 1. Basic validation
        if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
        {
            throw new ArgumentException("Invalid email address.");
        }

        // 2. Check if already subscribed
        var existing = await newsletterRepository.GetSubscriberByEmailAsync(email, cancellationToken);

        if (existing != null && existing.IsConfirmed)
        {
            // Already confirmed - return success (idempotent)
            return;
        }

        string token;
        if (existing != null)
        {
            // Resend confirmation if not confirmed
            token = Guid.NewGuid().ToString();
            existing.ConfirmationToken = token;
            await newsletterRepository.UpdateSubscriberAsync(existing, cancellationToken);
        }
        else
        {
            // New subscriber
            token = Guid.NewGuid().ToString();
            var subscriber = new NewsletterSubscriber
            {
                Email = email,
                SubscribedAt = DateTime.UtcNow,
                IsActive = true,
                IsConfirmed = false,
                ConfirmationToken = token
            };
            await newsletterRepository.AddSubscriberAsync(subscriber, cancellationToken);
        }

        // 3. Send Confirmation Email (Double Opt-In)
        var request = httpContextAccessor.HttpContext?.Request;
        var baseUrl = request != null
            ? $"{request.Scheme}://{request.Host}"
            : "https://localhost:7073";

        var confirmationLink = $"{baseUrl}/newsletter/confirm?email={Uri.EscapeDataString(email)}&token={token}";

        await emailService.SendEmailAsync(
            email,
            "Confirm your Stanza Subscription",
            $"<h1>Confirmation Required</h1><p>Please click the link below to verify your email:</p><a href='{confirmationLink}'>Confirm Subscription</a>",
            cancellationToken);
    }

    public async Task<bool> ConfirmSubscriptionAsync(string email, string token, CancellationToken cancellationToken = default)
    {
        var subscriber = await newsletterRepository.GetSubscriberByEmailAsync(email, cancellationToken);
        if (subscriber == null) return false;

        // Check token
        if (subscriber.IsConfirmed) return true; // Already confirmed
        if (subscriber.ConfirmationToken != token) return false; // Invalid token

        // Confirm
        subscriber.IsConfirmed = true;
        subscriber.ConfirmationToken = null; // Clear token after use
        await newsletterRepository.UpdateSubscriberAsync(subscriber, cancellationToken);

        // Send Welcome Email
        await emailService.SendEmailAsync(
            email,
            "Welcome to Stanza!",
            "<h1>Welcome!</h1><p>Thank you for subscribing to Stanza. You are now verified!</p>",
            cancellationToken);

        return true;
    }
}
