using EventPlanning.Domain.Entities;
using EventPlanning.Domain.Enums;
using EventPlanning.Domain.Interfaces;
using EventPlanning.Domain.Models;
using EventPlanning.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using EventPlanning.Infrastructure.Specifications;

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
        var spec = new EventFilterSpecification(
            organizerId,
            viewerId,
            searchTerm,
            from,
            to,
            type,
            sortOrder,
            pageNumber,
            pageSize
        );

        var countQuery = context.Events.AsNoTracking();
        if (spec.Criteria != null) countQuery = countQuery.Where(spec.Criteria);
        var totalCount = await countQuery.CountAsync(cancellationToken);

        var query = SpecificationEvaluator<Event>.GetQuery(context.Events.AsNoTracking(), spec);
        var items = await query.ToListAsync(cancellationToken);

        return new PagedList<Event>(items, totalCount, pageNumber, pageSize);
    }


    public async Task<bool> HasEventsAtVenueAsync(Guid venueId, CancellationToken cancellationToken = default)
    {
        return await context.Events
            .AsNoTracking()
            .AnyAsync(e => e.VenueId == venueId, cancellationToken);
    }
}