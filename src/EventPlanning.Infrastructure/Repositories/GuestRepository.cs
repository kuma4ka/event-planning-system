using EventPlanning.Domain.Entities;
using EventPlanning.Domain.Interfaces;
using EventPlanning.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EventPlanning.Infrastructure.Repositories;

public class GuestRepository(ApplicationDbContext context) : IGuestRepository
{
    public async Task<Guest?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await context.Guests
            .Include(g => g.Event) 
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }

    public async Task AddAsync(Guest guest, CancellationToken cancellationToken = default)
    {
        await context.Guests.AddAsync(guest, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guest guest, CancellationToken cancellationToken = default)
    {
        context.Guests.Remove(guest);
        await context.SaveChangesAsync(cancellationToken);
    }
}