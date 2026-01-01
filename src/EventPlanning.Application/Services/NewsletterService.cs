using EventPlanning.Application.Interfaces;
using EventPlanning.Domain.Entities;
using EventPlanning.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace EventPlanning.Application.Services;

public class NewsletterService(
    INewsletterRepository newsletterRepository,
    IEmailService emailService,
    IHttpContextAccessor httpContextAccessor,
    ILogger<NewsletterService> logger) : INewsletterService
{
    public async Task SubscribeAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
        {
            throw new ArgumentException("Invalid email address.");
        }

        var existing = await newsletterRepository.GetSubscriberByEmailAsync(email, cancellationToken);

        if (existing != null && existing.IsConfirmed)
        {
            return;
        }

        string token;
        if (existing != null)
        {
            token = Guid.NewGuid().ToString();
            existing.ConfirmationToken = token;
            await newsletterRepository.UpdateSubscriberAsync(existing, cancellationToken);
        }
        else
        {
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

        if (subscriber.IsConfirmed) return true;
        if (subscriber.ConfirmationToken != token)
        {
            logger.LogWarning("Invalid confirmation token provided for {Email}", email);
            return false;
        }

        subscriber.IsConfirmed = true;
        subscriber.ConfirmationToken = null;
        await newsletterRepository.UpdateSubscriberAsync(subscriber, cancellationToken);

        await emailService.SendEmailAsync(
            email,
            "Welcome to Stanza!",
            "<h1>Welcome!</h1><p>Thank you for subscribing to Stanza. You are now verified!</p>",
            cancellationToken);

        return true;
    }
}
