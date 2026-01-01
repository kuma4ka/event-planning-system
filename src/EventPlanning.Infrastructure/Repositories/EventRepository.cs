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

    public async Task<int> CountGuestsAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return await context.Guests
           .AsNoTracking()
           .CountAsync(g => g.EventId == eventId, cancellationToken);
    }

    public async Task<bool> GuestEmailExistsAsync(Guid eventId, string email, Guid? excludeGuestId = null, CancellationToken cancellationToken = default)
    {
        var query = context.Guests
            .AsNoTracking()
            .Where(g => g.EventId == eventId && (string)g.Email == email);

        if (excludeGuestId.HasValue)
        {
            query = query.Where(g => g.Id != excludeGuestId);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> GuestPhoneExistsAsync(Guid eventId, string phoneNumber, Guid? excludeGuestId = null, CancellationToken cancellationToken = default)
    {
        PhoneNumber? phoneVo;
        try
        {
            phoneVo = PhoneNumber.Create(phoneNumber);
        }
        catch (ArgumentException)
        {
            return false;
        }

        var query = context.Guests
            .AsNoTracking()
            .Where(g => g.EventId == eventId && g.PhoneNumber == phoneVo);

        if (excludeGuestId.HasValue)
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

    public async Task<bool> IsUserJoinedAsync(Guid eventId, string userId, CancellationToken cancellationToken = default)
    {
        var user = await context.Users.FindAsync([userId], cancellationToken);
        if (user == null || string.IsNullOrEmpty(user.Email)) return false;

        return await context.Guests
            .AnyAsync(g => g.EventId == eventId && (string)g.Email == user.Email, cancellationToken);
    }

    public async Task AddGuestAsync(Guest guest, CancellationToken cancellationToken = default)
    {
        await context.Guests.AddAsync(guest, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveGuestAsync(Guid eventId, string userId, CancellationToken cancellationToken = default)
    {
        var user = await context.Users.FindAsync([userId], cancellationToken);
        if (user == null || string.IsNullOrEmpty(user.Email)) return;

        var guest = await context.Guests
            .FirstOrDefaultAsync(g => g.EventId == eventId && (string)g.Email == user.Email, cancellationToken);

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
            .CountAsync(g => (string)g.Email == user.Email, cancellationToken);
    }

    public async Task<bool> HasEventsAtVenueAsync(Guid venueId, CancellationToken cancellationToken = default)
    {
        return await context.Events
            .AsNoTracking()
            .AnyAsync(e => e.VenueId == venueId, cancellationToken);
    }

    public async Task<bool> TryJoinEventAsync(Guest guest, CancellationToken cancellationToken = default)
    {
        var strategy = context.Database.CreateExecutionStrategy();
        
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
            try
            {
                var eventEntity = await context.Events
                    .Include(e => e.Venue)
                    .FirstOrDefaultAsync(e => e.Id == guest.EventId, cancellationToken);
                
                if (eventEntity == null) return false;

                if (eventEntity.VenueId.HasValue && eventEntity.Venue != null && eventEntity.Venue.Capacity > 0)
                {
                   var currentCount = await context.Guests
                       .CountAsync(g => g.EventId == guest.EventId, cancellationToken);
                    
                   if (eventEntity.IsFull(currentCount))
                   {
                       return false;
                   }
                }

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
        });
    }
    public async Task<List<Guid>> UpdateGuestDetailsAsync(string email, string firstName, string lastName, string countryCode, string? phoneNumber, CancellationToken cancellationToken = default)
    {
        var guests = await context.Guests
            .Where(g => (string)g.Email == email)
            .ToListAsync(cancellationToken);

        var affectedEventIds = new List<Guid>();

        foreach (var guest in guests)
        {
            guest.UpdateDetails(firstName, lastName, email, countryCode, phoneNumber);
            affectedEventIds.Add(guest.EventId);
        }

        if (guests.Any())
        {
            await context.SaveChangesAsync(cancellationToken);
        }

        return affectedEventIds;
    }
}