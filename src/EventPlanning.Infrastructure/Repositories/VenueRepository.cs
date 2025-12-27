using EventPlanning.Domain.Entities;
using EventPlanning.Domain.Interfaces;
using EventPlanning.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EventPlanning.Infrastructure.Repositories;

public class VenueRepository(ApplicationDbContext context) : IVenueRepository
{
    public async Task<Venue?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await context.Venues
            .Include(v => v.Events)
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    public async Task<List<Venue>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Venues
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Venue venue, CancellationToken cancellationToken = default)
    {
        await context.Venues.AddAsync(venue, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Venue venue, CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Venue venue, CancellationToken cancellationToken = default)
    {
        context.Venues.Remove(venue);
        await context.SaveChangesAsync(cancellationToken);
    }
}