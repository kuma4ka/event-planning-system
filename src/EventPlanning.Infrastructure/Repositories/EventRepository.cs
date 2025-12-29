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
            // .Include(e => e.Guests) // Removed for performance (lazy loading split)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<Event?> GetDetailsByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await context.Events
            .Include(e => e.Venue)
            .Include(e => e.Guests)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<int> CountGuestsAsync(int eventId, CancellationToken cancellationToken = default)
    {
        return await context.Guests
           .AsNoTracking()
           .CountAsync(g => g.EventId == eventId, cancellationToken);
    }

    public async Task<bool> GuestEmailExistsAsync(int eventId, string email, string? excludeGuestId = null, CancellationToken cancellationToken = default)
    {
        var query = context.Guests
            .AsNoTracking()
            .Where(g => g.EventId == eventId && g.Email == email);

        if (!string.IsNullOrEmpty(excludeGuestId))
        {
            query = query.Where(g => g.Id != excludeGuestId);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> GuestPhoneExistsAsync(int eventId, string phoneNumber, string? excludeGuestId = null, CancellationToken cancellationToken = default)
    {
        var query = context.Guests
            .AsNoTracking()
            .Where(g => g.EventId == eventId && g.PhoneNumber == phoneNumber);

        if (!string.IsNullOrEmpty(excludeGuestId))
        {
            query = query.Where(g => g.Id != excludeGuestId);
        }

        return await query.AnyAsync(cancellationToken);
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

        query = !string.IsNullOrEmpty(viewerId)
            ? query.Where(e => !e.IsPrivate || e.OrganizerId == viewerId)
            : query.Where(e => !e.IsPrivate);

        if (!string.IsNullOrEmpty(organizerId))
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

    public async Task<bool> IsUserJoinedAsync(int eventId, string userId, CancellationToken cancellationToken = default)
    {
        var user = await context.Users.FindAsync([userId], cancellationToken);
        if (user == null || string.IsNullOrEmpty(user.Email)) return false;

        return await context.Guests
            .AnyAsync(g => g.EventId == eventId && g.Email == user.Email, cancellationToken);
    }

    public async Task<bool> TryJoinEventAsync(int eventId, string userId, CancellationToken cancellationToken = default)
    {
        // Use a Serializable transaction to prevent race conditions (overbooking)
        // This ensures that between reading the count and inserting, no other transaction can modify the data.
        using var transaction = await context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);

        try
        {
            var user = await context.Users.FindAsync([userId], cancellationToken);
            if (user == null || string.IsNullOrEmpty(user.Email)) return false;

            var eventEntity = await context.Events
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(e => e.Id == eventId, cancellationToken);

            if (eventEntity == null) return false;

            // Check capacity
            if (eventEntity.Venue is { Capacity: > 0 })
            {
                // We must query the count from the DB within this transaction scope
                var currentCount = await context.Guests.CountAsync(g => g.EventId == eventId, cancellationToken);
                if (currentCount >= eventEntity.Venue.Capacity)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return false; // Event is full
                }
            }

            // Check if already joined
            var alreadyJoined = await context.Guests.AnyAsync(g => g.EventId == eventId && g.Email == user.Email, cancellationToken);
            if (alreadyJoined)
            {
                await transaction.RollbackAsync(cancellationToken);
                return false; // Already joined
            }

            var guest = new Guest(
                Guid.NewGuid().ToString(),
                user.FirstName,
                user.LastName,
                user.Email,
                eventId,
                user.PhoneNumber
            );

            await context.Guests.AddAsync(guest, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return true;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task RemoveGuestAsync(int eventId, string userId, CancellationToken cancellationToken = default)
    {
        var user = await context.Users.FindAsync([userId], cancellationToken);
        if (user == null || string.IsNullOrEmpty(user.Email)) return;

        var guest = await context.Guests
            .FirstOrDefaultAsync(g => g.EventId == eventId && g.Email == user.Email, cancellationToken);

        if (guest != null)
        {
            context.Guests.Remove(guest);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<int> CountJoinedEventsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await context.Users.FindAsync([userId], cancellationToken);
        if (user == null || string.IsNullOrEmpty(user.Email)) return 0;

        return await context.Guests
            .AsNoTracking()
            .CountAsync(g => g.Email == user.Email, cancellationToken);
    }
}