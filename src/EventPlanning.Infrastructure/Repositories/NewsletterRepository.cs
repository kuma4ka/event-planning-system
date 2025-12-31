using EventPlanning.Domain.Entities;
using EventPlanning.Domain.Interfaces;
using EventPlanning.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EventPlanning.Infrastructure.Repositories;

public class NewsletterRepository(ApplicationDbContext context) : INewsletterRepository
{
    public async Task<bool> IsEmailSubscribedAsync(string email, CancellationToken cancellationToken = default)
    {
        return await context.NewsletterSubscribers
            .AnyAsync(s => s.Email == email, cancellationToken);
    }

    public async Task AddSubscriberAsync(NewsletterSubscriber subscriber, CancellationToken cancellationToken = default)
    {
        await context.NewsletterSubscribers.AddAsync(subscriber, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
