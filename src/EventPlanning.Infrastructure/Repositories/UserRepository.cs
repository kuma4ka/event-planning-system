using EventPlanning.Domain.Interfaces;
using EventPlanning.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EventPlanning.Infrastructure.Repositories;

public class UserRepository(ApplicationDbContext context) : IUserRepository
{
    public async Task<bool> IsPhoneNumberTakenAsync(string phoneNumber, Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            var phone = EventPlanning.Domain.ValueObjects.PhoneNumber.Create(phoneNumber);
            return await context.Users
                .AnyAsync(u => u.PhoneNumber == phone && u.Id != userId, cancellationToken);
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    public async Task<EventPlanning.Domain.Entities.User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await context.Users.FindAsync([userId], cancellationToken);
    }

    public async Task<EventPlanning.Domain.Entities.User?> GetByIdentityIdAsync(string identityId, CancellationToken cancellationToken)
    {
        return await context.Users
            .FirstOrDefaultAsync(u => u.IdentityId == identityId, cancellationToken);
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