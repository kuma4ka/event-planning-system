using EventPlanning.Domain.Entities;
using EventPlanning.Domain.Enums;
using EventPlanning.Domain.Interfaces;
using EventPlanning.Domain.Models;
using EventPlanning.Domain.ValueObjects;
using EventPlanning.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using EventPlanning.Infrastructure.Extensions;

namespace EventPlanning.Infrastructure.Repositories;

public class EventRepository(ApplicationDbContext context) : IEventRepository
{
    public async Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Events
            .Include(e => e.Venue)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<Event?> GetDetailsByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Events
            .Include(e => e.Venue)
            .Include(e => e.Guests)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }



    public async Task<List<Event>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Events
            .AsNoTracking()
            .Include(e => e.Venue)
            .OrderByDescending(e => e.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Event>> GetByOrganizerAsync(Guid organizerId,
        CancellationToken cancellationToken = default)
    {
        return await context.Events
            .AsNoTracking()
            .Where(e => e.OrganizerId == organizerId)
            .Include(e => e.Venue)
            .OrderByDescending(e => e.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task<Guid> AddAsync(Event eventEntity, CancellationToken cancellationToken = default)
    {
        await context.Events.AddAsync(eventEntity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return eventEntity.Id;
    }

    public async Task UpdateAsync(Event eventEntity, CancellationToken cancellationToken = default)
    {
        context.Events.Update(eventEntity);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Event eventEntity, CancellationToken cancellationToken = default)
    {
        context.Events.Remove(eventEntity);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedList<Event>> GetFilteredAsync(
        Guid? organizerId,
        Guid? viewerId,
        string? searchTerm,
        DateTime? from,
        DateTime? to,
        EventType? type,
        string? sortOrder,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = context.Events
            .Include(e => e.Venue)
            .AsNoTracking()
            .AsQueryable();

        query = viewerId.HasValue
            ? query.Where(e => !e.IsPrivate || e.OrganizerId == viewerId)
            : query.Where(e => !e.IsPrivate);

        if (organizerId.HasValue)
            query = query.Where(e => e.OrganizerId == organizerId);

        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(e =>
                e.Name.Contains(searchTerm) || (e.Description != null && e.Description.Contains(searchTerm)));

        if (from.HasValue) query = query.Where(e => e.Date >= from.Value);
        if (to.HasValue) query = query.Where(e => e.Date <= to.Value);
        if (type.HasValue) query = query.Where(e => e.Type == type.Value);

        query = sortOrder switch
        {
            "name_asc" => query.OrderBy(e => e.Name),
            "name_desc" => query.OrderByDescending(e => e.Name),
            "date_asc" => query.OrderBy(e => e.Date),
            "newest" => query.OrderByDescending(e => e.CreatedAt),
            _ => query.OrderByDescending(e => e.Date)
        };

        return await query.ToPagedListAsync(pageNumber, pageSize, cancellationToken);
    }



    public async Task<bool> HasEventsAtVenueAsync(Guid venueId, CancellationToken cancellationToken = default)
    {
        return await context.Events
            .AsNoTracking()
            .AnyAsync(e => e.VenueId == venueId, cancellationToken);
    }


}