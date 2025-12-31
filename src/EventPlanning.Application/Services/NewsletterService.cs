using EventPlanning.Application.Interfaces;
using EventPlanning.Domain.Entities;
using EventPlanning.Domain.Interfaces;

namespace EventPlanning.Application.Services;

public class NewsletterService(INewsletterRepository newsletterRepository) : INewsletterService
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
            // Already subscribed - technically success for idempotency, or throw explicit error
            // Taking friendly approach: just return, maybe logic elsewhere handles "already exists" UI
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
    }
}
