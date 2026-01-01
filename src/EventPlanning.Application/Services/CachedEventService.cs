using EventPlanning.Application.DTOs.Event;
using EventPlanning.Application.DTOs.Guest;
using EventPlanning.Application.Interfaces;
using EventPlanning.Application.Models;

namespace EventPlanning.Application.Services;

public class CachedEventService(
    IEventService innerService,
    ICacheService cache) : IEventService
{
    private const string EventCacheKeyPrefix = "event_details_";

    public Task<PagedResult<EventDto>> GetEventsAsync(string userId, string? organizerIdFilter, EventSearchDto searchDto, string? sortOrder, CancellationToken cancellationToken = default)
    {
        return innerService.GetEventsAsync(userId, organizerIdFilter, searchDto, sortOrder, cancellationToken);
    }

    public Task<EventDto?> GetEventByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return innerService.GetEventByIdAsync(id, cancellationToken);
    }

    public async Task<EventDetailsDto?> GetEventDetailsAsync(Guid id, string? userId, CancellationToken cancellationToken = default)
    {
        var publicCacheKey = $"{EventCacheKeyPrefix}{id}_public";
        var organizerCacheKey = $"{EventCacheKeyPrefix}{id}_organizer";

        if (cache.Get<EventDetailsDto>(organizerCacheKey) is { } organizerCachedEvent)
        {
            // If I am the organizer and I found my cached copy
            if (organizerCachedEvent.OrganizerId == userId)
            {
                return organizerCachedEvent;
            }
        }

        if (cache.Get<EventDetailsDto>(publicCacheKey) is { } publicCachedEvent)
        {
            // If I am NOT the organizer (or userId is null), safe to return public copy
            if (publicCachedEvent.OrganizerId != userId)
            {
                return publicCachedEvent;
            }
        }

        var result = await innerService.GetEventDetailsAsync(id, userId, cancellationToken);

        if (result != null)
        {
            var slidingExpiration = TimeSpan.FromMinutes(10);
            var absoluteExpiration = TimeSpan.FromHours(1);

            if (result.IsOrganizer)
            {
                cache.Set(organizerCacheKey, result, slidingExpiration, absoluteExpiration);
            }
            else
            {
                cache.Set(publicCacheKey, result, slidingExpiration, absoluteExpiration);
            }
        }

        return result;
    }

    public async Task<Guid> CreateEventAsync(string userId, CreateEventDto dto, CancellationToken cancellationToken = default)
    {
        return await innerService.CreateEventAsync(userId, dto, cancellationToken);
    }

    public async Task UpdateEventAsync(string userId, UpdateEventDto dto, CancellationToken cancellationToken = default)
    {
        await innerService.UpdateEventAsync(userId, dto, cancellationToken);
        InvalidateEventCache(dto.Id);
    }

    public async Task DeleteEventAsync(string userId, Guid eventId, CancellationToken cancellationToken = default)
    {
        await innerService.DeleteEventAsync(userId, eventId, cancellationToken);
        InvalidateEventCache(eventId);
    }

    public async Task JoinEventAsync(Guid eventId, string userId, CancellationToken cancellationToken = default)
    {
        await innerService.JoinEventAsync(eventId, userId, cancellationToken);
        InvalidateEventCache(eventId);
    }

    public async Task LeaveEventAsync(Guid eventId, string userId, CancellationToken cancellationToken = default)
    {
        await innerService.LeaveEventAsync(eventId, userId, cancellationToken);
        InvalidateEventCache(eventId);
    }

    public Task<bool> IsUserJoinedAsync(Guid eventId, string userId, CancellationToken cancellationToken = default)
    {
        return innerService.IsUserJoinedAsync(eventId, userId, cancellationToken);
    }

    private void InvalidateEventCache(Guid eventId)
    {
        cache.Remove($"{EventCacheKeyPrefix}{eventId}_public");
        cache.Remove($"{EventCacheKeyPrefix}{eventId}_organizer");
    }
}
