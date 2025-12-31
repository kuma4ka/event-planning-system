using EventPlanning.Application.Interfaces;
using EventPlanning.Domain.Entities;
using EventPlanning.Domain.Interfaces;

namespace EventPlanning.Application.Services;

public class NewsletterService(INewsletterRepository newsletterRepository, IEmailService emailService) : INewsletterService
{
    public async Task SubscribeAsync(string email, CancellationToken cancellationToken = default)
    {
        // 1. Basic validation
        if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
        {
            throw new ArgumentException("Invalid email address.");
        }

        // 2. Check if already subscribed
        if (await newsletterRepository.IsEmailSubscribedAsync(email, cancellationToken))
        {
            // Already subscribed - technically success for idempotency
            return;
        }

        // 3. Create entity
        var subscriber = new NewsletterSubscriber
        {
            Email = email,
            SubscribedAt = DateTime.UtcNow,
            IsActive = true
        };

        // 4. Save
        await newsletterRepository.AddSubscriberAsync(subscriber, cancellationToken);

        // 5. Send Welcome Email
        await emailService.SendEmailAsync(
            email,
            "Welcome to Stanza!",
            "<h1>Welcome!</h1><p>Thank you for subscribing to Stanza. We are excited to have you.</p>",
            cancellationToken);
    }
}
