using EventPlanning.Application.Interfaces;
using EventPlanning.Domain.Entities;
using EventPlanning.Domain.Interfaces;
using EventPlanning.Application.Constants;
using Microsoft.Extensions.Logging;

namespace EventPlanning.Application.Services;

public class EventParticipationService(
    IEventRepository eventRepository,
    IGuestRepository guestRepository,
    IUserRepository userRepository,
    ICacheService cacheService,
    ILogger<EventParticipationService> logger) : IEventParticipationService
{
    public async Task JoinEventAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default)
    {
        var eventEntity = await eventRepository.GetByIdAsync(eventId, cancellationToken);
        if (eventEntity == null) throw new KeyNotFoundException($"Event {eventId} not found");

        if (eventEntity.Date < DateTime.UtcNow)
            throw new InvalidOperationException("Cannot join an event that has already ended.");

        var user = await userRepository.GetByIdentityIdAsync(userId.ToString(), cancellationToken);
        if (user == null) throw new KeyNotFoundException($"User profile not found for identity {userId}");

        if (eventEntity.OrganizerId == user.Id)
            throw new InvalidOperationException("You cannot join your own event as a guest.");

        var emailExists = await guestRepository.EmailExistsAtEventAsync(eventId, user.Email, null, cancellationToken);
        if (emailExists) throw new InvalidOperationException("You are already registered for this event.");

        if (user.PhoneNumber != null)
        {
             var phoneExists = await guestRepository.PhoneExistsAtEventAsync(eventId, user.PhoneNumber.Value, null, cancellationToken);
             if (phoneExists)  throw new InvalidOperationException("You are already registered for this event.");
        }

        var guest = new Guest(
            user.FirstName,
            user.LastName,
            user.Email,
            eventId,
            user.CountryCode,
            user.PhoneNumber?.Value,
            user.Id
        );

        var success = await guestRepository.TryJoinEventAsync(guest, cancellationToken);
        if (!success)
        {
             logger.LogWarning("Join failed: Event {EventId} is full.", eventId);
             throw new InvalidOperationException("Sorry, this event is fully booked or unavailable.");
        }

        InvalidateEventCache(eventId);
    }

    public async Task LeaveEventAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdentityIdAsync(userId.ToString(), cancellationToken);
        if (user == null) return;

        await guestRepository.RemoveGuestByUserIdAsync(eventId, user.Id, cancellationToken);
        InvalidateEventCache(eventId);
    }

    public async Task<bool> IsUserJoinedAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdentityIdAsync(userId.ToString(), cancellationToken);
        if (user == null) return false;

        return await guestRepository.IsUserJoinedAsync(eventId, user.Id, cancellationToken);
    }

    private void InvalidateEventCache(Guid eventId)
    {
        cacheService.Remove(CacheKeyGenerator.GetEventKeyPublic(eventId));
        cacheService.Remove(CacheKeyGenerator.GetEventKeyOrganizer(eventId));
    }
}
