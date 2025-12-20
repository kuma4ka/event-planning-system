using EventPlanning.Domain.Entities;
using EventPlanning.Domain.Enums;
using EventPlanning.Domain.Interfaces;
using EventPlanning.Domain.Models;
using EventPlanning.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using EventPlanning.Infrastructure.Extensions;

namespace EventPlanning.Infrastructure.Repositories;

public class EventRepository(ApplicationDbContext context) : IEventRepository
{
    public async Task<Event?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
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

    public async Task<List<Event>> GetByOrganizerAsync(string organizerId,
        CancellationToken cancellationToken = default)
    {
        return await context.Events
            .AsNoTracking()
            .Where(e => e.OrganizerId == organizerId)
            .Include(e => e.Venue)
            .OrderByDescending(e => e.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> AddAsync(Event eventEntity, CancellationToken cancellationToken = default)
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
        string? organizerId, 
        string? viewerId,
        string? searchTerm, 
        DateTime? from, 
        DateTime? to, 
        EventType? type, 
        int pageNumber, 
        int pageSize, 
        CancellationToken cancellationToken = default)
    {
        var query = context.Events
            .Include(e => e.Venue)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrEmpty(viewerId))
        {
            query = query.Where(e => !e.IsPrivate || e.OrganizerId == viewerId);
        }
        else
        {
            query = query.Where(e => !e.IsPrivate);
        }

        if (!string.IsNullOrEmpty(organizerId))
        {
            query = query.Where(e => e.OrganizerId == organizerId);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(e => e.Name.Contains(searchTerm) || (e.Description != null && e.Description.Contains(searchTerm)));
        }

        if (from.HasValue) query = query.Where(e => e.Date >= from.Value);
        if (to.HasValue) query = query.Where(e => e.Date <= to.Value);
        if (type.HasValue) query = query.Where(e => e.Type == type.Value);

        query = query.OrderBy(e => e.Date);

        return await query.ToPagedListAsync(pageNumber, pageSize, cancellationToken);
    }

    public async Task<bool> IsUserJoinedAsync(int eventId, string userId, CancellationToken cancellationToken = default)
    {
        return await context.Events
            .Include(e => e.Guests)
            .AnyAsync(e => e.Id == eventId && e.Guests.Any(g => g.Id == userId), cancellationToken);
    }

    public async Task AddGuestAsync(int eventId, string userId, CancellationToken cancellationToken = default)
    {
        var eventEntity = await context.Events
            .Include(e => e.Guests)
            .FirstOrDefaultAsync(e => e.Id == eventId, cancellationToken);

        if (eventEntity == null) throw new KeyNotFoundException("Event not found");

        var identityUser = await context.Users.FindAsync(new object[] { userId }, cancellationToken);
        if (identityUser == null) throw new KeyNotFoundException("User not found");

        if (!eventEntity.Guests.Any(g => g.Id == userId))
        {
            var newGuest = new Guest
            {
                Id = identityUser.Id,
                FirstName = identityUser.FirstName,
                LastName = identityUser.LastName,
                Email = identityUser.Email!,
                PhoneNumber = identityUser.PhoneNumber,
                EventId = eventId
            };

            eventEntity.Guests.Add(newGuest);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task RemoveGuestAsync(int eventId, string userId, CancellationToken cancellationToken = default)
    {
        var eventEntity = await context.Events
            .Include(e => e.Guests)
            .FirstOrDefaultAsync(e => e.Id == eventId, cancellationToken);

        if (eventEntity == null) return;

        var guest = eventEntity.Guests.FirstOrDefault(g => g.Id == userId);
        if (guest != null)
        {
            eventEntity.Guests.Remove(guest);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}