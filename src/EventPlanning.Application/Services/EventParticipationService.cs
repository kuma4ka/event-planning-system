using EventPlanning.Application.Interfaces;
using EventPlanning.Domain.Entities;
using EventPlanning.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EventPlanning.Application.Services;

public class EventParticipationService(
    IEventRepository eventRepository,
    IUserRepository userRepository,
    ICacheService cacheService,
    ILogger<EventParticipationService> logger) : IEventParticipationService
{
    public async Task JoinEventAsync(Guid eventId, string userId, CancellationToken cancellationToken = default)
    {
        var eventEntity = await eventRepository.GetByIdAsync(eventId, cancellationToken);
        if (eventEntity == null) throw new KeyNotFoundException($"Event {eventId} not found");

        if (eventEntity.Date < DateTime.UtcNow)
            throw new InvalidOperationException("Cannot join an event that has already ended.");

        if (eventEntity.OrganizerId == userId)
            throw new InvalidOperationException("You cannot join your own event as a guest.");

        var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null) throw new KeyNotFoundException($"User {userId} not found");

        // Check if already joined
        var emailExists = await eventRepository.GuestEmailExistsAsync(eventId, user.Email!, null, cancellationToken);
        if (emailExists) throw new InvalidOperationException("You are already registered for this event.");

        if (!string.IsNullOrEmpty(user.PhoneNumber))
        {
             var phoneExists = await eventRepository.GuestPhoneExistsAsync(eventId, user.PhoneNumber, null, cancellationToken);
             if (phoneExists)  throw new InvalidOperationException("You are already registered for this event.");
        }

        var guest = new Guest(
            user.FirstName,
            user.LastName,
            user.Email!,
            eventId,
            user.CountryCode,
            user.PhoneNumber,
            user.Id
        );

        // Attempt to join transactionally
        var success = await eventRepository.TryJoinEventAsync(guest, cancellationToken);
        if (!success)
        {
             logger.LogWarning("Join failed: Event {EventId} is full.", eventId);
             throw new InvalidOperationException("Sorry, this event is fully booked or unavailable.");
        }

        InvalidateEventCache(eventId);
    }

    public async Task LeaveEventAsync(Guid eventId, string userId, CancellationToken cancellationToken = default)
    {
        await eventRepository.RemoveGuestAsync(eventId, userId, cancellationToken);
        InvalidateEventCache(eventId);
    }

    public async Task<bool> IsUserJoinedAsync(Guid eventId, string userId, CancellationToken cancellationToken = default)
    {
        return await eventRepository.IsUserJoinedAsync(eventId, userId, cancellationToken);
    }

    private void InvalidateEventCache(Guid eventId)
    {
        cacheService.Remove($"{CachedEventService.EventCacheKeyPrefix}{eventId}_public");
        cacheService.Remove($"{CachedEventService.EventCacheKeyPrefix}{eventId}_organizer");
    }
}
