using EventPlanning.Domain.Entities;

namespace EventPlanning.Domain.Interfaces;

public interface INewsletterRepository
{
    Task<bool> IsEmailSubscribedAsync(string email, CancellationToken cancellationToken = default);
    Task AddSubscriberAsync(NewsletterSubscriber subscriber, CancellationToken cancellationToken = default);
}
