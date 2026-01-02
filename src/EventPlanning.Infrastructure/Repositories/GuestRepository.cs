using EventPlanning.Domain.Entities;
using EventPlanning.Domain.Interfaces;
using EventPlanning.Domain.ValueObjects;
using EventPlanning.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EventPlanning.Infrastructure.Repositories;

public class GuestRepository(ApplicationDbContext context) : IGuestRepository
{
    public async Task<Guest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
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

    public async Task UpdateAsync(Guest guest, CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guest guest, CancellationToken cancellationToken = default)
    {
        context.Guests.Remove(guest);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> IsUserJoinedAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await context.Users.FindAsync([userId], cancellationToken);
        if (user == null || string.IsNullOrEmpty(user.Email)) return false;

        return await context.Guests
            .AnyAsync(g => g.EventId == eventId && (string)g.Email == user.Email, cancellationToken);
    }

    public async Task RemoveGuestByUserIdAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default)
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

    public async Task<int> CountJoinedEventsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await context.Users.FindAsync([userId], cancellationToken);
        if (user == null || string.IsNullOrEmpty(user.Email)) return 0;

        return await context.Guests
            .AsNoTracking()
            .CountAsync(g => (string)g.Email == user.Email, cancellationToken);
    }

    public async Task<int> CountGuestsAtEventAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return await context.Guests
           .AsNoTracking()
           .CountAsync(g => g.EventId == eventId, cancellationToken);
    }

    public async Task<bool> EmailExistsAtEventAsync(Guid eventId, string email, Guid? excludeGuestId = null, CancellationToken cancellationToken = default)
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

    public async Task<bool> PhoneExistsAtEventAsync(Guid eventId, string phoneNumber, Guid? excludeGuestId = null, CancellationToken cancellationToken = default)
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

    public async Task<List<Guid>> UpdateGuestDetailsByEmailAsync(string email, string firstName, string lastName, string countryCode, string? phoneNumber, CancellationToken cancellationToken = default)
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