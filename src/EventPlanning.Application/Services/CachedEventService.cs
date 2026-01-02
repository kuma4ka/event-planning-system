using EventPlanning.Application.DTOs.Event;
using EventPlanning.Application.Constants;
using EventPlanning.Application.Interfaces;
using Mapster;
using Microsoft.Extensions.Caching.Memory;
using EventPlanning.Application.Models;

namespace EventPlanning.Application.Services;

public class CachedEventService(
    IEventService innerService,
    ICacheService cache) : IEventService
{
    public const string EventCacheKeyPrefix = "event_details_";

    public Task<PagedResult<EventDto>> GetEventsAsync(Guid userId, Guid? organizerIdFilter, EventSearchDto searchDto, string? sortOrder, CancellationToken cancellationToken = default)
    {
        return innerService.GetEventsAsync(userId, organizerIdFilter, searchDto, sortOrder, cancellationToken);
    }

    public Task<EventDto?> GetEventByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return innerService.GetEventByIdAsync(id, cancellationToken);
    }

    public async Task<EventDetailsDto?> GetEventDetailsAsync(Guid id, Guid? userId, CancellationToken cancellationToken = default)
    {
        var publicCacheKey = CacheKeyGenerator.GetEventKeyPublic(id);
        var organizerCacheKey = CacheKeyGenerator.GetEventKeyOrganizer(id);

        if (cache.Get<EventDetailsDto>(publicCacheKey) is { } publicCachedEvent)
        {
            if (!userId.HasValue || publicCachedEvent.OrganizerId != userId.Value)
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

    public async Task<Guid> CreateEventAsync(Guid userId, CreateEventDto dto, CancellationToken cancellationToken = default)
    {
        return await innerService.CreateEventAsync(userId, dto, cancellationToken);
    }

    public async Task UpdateEventAsync(Guid userId, UpdateEventDto dto, CancellationToken cancellationToken = default)
    {
        await innerService.UpdateEventAsync(userId, dto, cancellationToken);
        InvalidateEventCache(dto.Id);
    }

    public async Task DeleteEventAsync(Guid userId, Guid eventId, CancellationToken cancellationToken = default)
    {
        await innerService.DeleteEventAsync(userId, eventId, cancellationToken);
        InvalidateEventCache(eventId);
    }



    private void InvalidateEventCache(Guid eventId)
    {
        cache.Remove(CacheKeyGenerator.GetEventKeyPublic(eventId));
        cache.Remove(CacheKeyGenerator.GetEventKeyOrganizer(eventId));
    }
}
