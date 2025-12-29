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

    public async Task<(List<Venue> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = context.Venues.AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(v => v.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
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