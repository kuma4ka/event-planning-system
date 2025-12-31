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
}