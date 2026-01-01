using EventPlanning.Domain.Interfaces;
using EventPlanning.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EventPlanning.Infrastructure.Repositories;

public class UserRepository(ApplicationDbContext context) : IUserRepository
{
    public async Task<bool> IsPhoneNumberTakenAsync(string phoneNumber, string userId, CancellationToken cancellationToken)
    {
        return await context.Users
            .AnyAsync(u => u.PhoneNumber == phoneNumber && u.Id != userId, cancellationToken);
    }

    public async Task<EventPlanning.Domain.Entities.User?> GetByIdAsync(string userId, CancellationToken cancellationToken)
    {
        return await context.Users.FindAsync([userId], cancellationToken);
    }

    public async Task AddAsync(EventPlanning.Domain.Entities.User user, CancellationToken cancellationToken = default)
    {
        await context.Users.AddAsync(user, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(EventPlanning.Domain.Entities.User user, CancellationToken cancellationToken = default)
    {
        context.Users.Update(user);
        await context.SaveChangesAsync(cancellationToken);
    }
}