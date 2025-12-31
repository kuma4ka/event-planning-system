using EventPlanning.Domain.Entities;
using EventPlanning.Domain.Interfaces;
using EventPlanning.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EventPlanning.Infrastructure.Repositories;

public class NewsletterRepository(ApplicationDbContext context) : INewsletterRepository
{
    public async Task<bool> IsEmailSubscribedAsync(string email, CancellationToken cancellationToken = default)
    {
        return await context.Set<NewsletterSubscriber>()
            .AnyAsync(s => s.Email == email && s.IsConfirmed, cancellationToken);
    }

    public async Task AddSubscriberAsync(NewsletterSubscriber subscriber, CancellationToken cancellationToken = default)
    {
        await context.Set<NewsletterSubscriber>().AddAsync(subscriber, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<NewsletterSubscriber?> GetSubscriberByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await context.Set<NewsletterSubscriber>()
            .FirstOrDefaultAsync(s => s.Email == email, cancellationToken);
    }

    public async Task UpdateSubscriberAsync(NewsletterSubscriber subscriber, CancellationToken cancellationToken = default)
    {
        context.Set<NewsletterSubscriber>().Update(subscriber);
        await context.SaveChangesAsync(cancellationToken);
    }
}
