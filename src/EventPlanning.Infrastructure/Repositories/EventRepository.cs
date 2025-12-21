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
        string? sortOrder,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = context.Events
            .Include(e => e.Venue)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrEmpty(viewerId))
            query = query.Where(e => !e.IsPrivate || e.OrganizerId == viewerId);
        else
            query = query.Where(e => !e.IsPrivate);

        if (!string.IsNullOrEmpty(organizerId)) query = query.Where(e => e.OrganizerId == organizerId);

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

    public async Task<bool> IsUserJoinedAsync(int eventId, string userId, CancellationToken cancellationToken = default)
    {
        var user = await context.Users.FindAsync(new object[] { userId }, cancellationToken);
        if (user == null || string.IsNullOrEmpty(user.Email)) return false;

        return await context.Guests
            .AnyAsync(g => g.EventId == eventId && g.Email == user.Email, cancellationToken);
    }

    public async Task AddGuestAsync(int eventId, string userId, CancellationToken cancellationToken = default)
    {
        var user = await context.Users.FindAsync(new object[] { userId }, cancellationToken);
        if (user == null) throw new KeyNotFoundException("User not found");

        if (string.IsNullOrEmpty(user.Email))
            throw new InvalidOperationException("User does not have an email address.");

        var guest = new Guest
        {
            Id = Guid.NewGuid().ToString(),

            EventId = eventId,
            FirstName = user.FirstName ?? "Unknown",
            LastName = user.LastName ?? "User",

            Email = user.Email,

            PhoneNumber = user.PhoneNumber
        };

        await context.Guests.AddAsync(guest, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
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
    
    public async Task<int> CountJoinedEventsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await context.Users.FindAsync(new object[] { userId }, cancellationToken);
        if (user == null || string.IsNullOrEmpty(user.Email)) return 0;

        return await context.Guests
            .AsNoTracking()
            .CountAsync(g => g.Email == user.Email, cancellationToken);
    }
}