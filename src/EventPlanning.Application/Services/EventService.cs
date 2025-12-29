using EventPlanning.Application.DTOs.Event;
using EventPlanning.Application.DTOs.Guest;
using EventPlanning.Application.Constants;
using EventPlanning.Application.Interfaces;
using EventPlanning.Application.Models;
using EventPlanning.Domain.Entities;
using EventPlanning.Domain.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace EventPlanning.Application.Services;

public class EventService(
    IEventRepository eventRepository,
    IValidator<CreateEventDto> createValidator,
    IValidator<UpdateEventDto> updateValidator,
    IValidator<EventSearchDto> searchValidator,
    IHttpContextAccessor httpContextAccessor,
    IMemoryCache cache) : IEventService
{
    private const string EventCacheKeyPrefix = "event_details_";

    public async Task<PagedResult<EventDto>> GetEventsAsync(
        string userId,
        string? organizerIdFilter,
        EventSearchDto searchDto,
        string? sortOrder,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await searchValidator.ValidateAsync(searchDto, cancellationToken);
        if (!validationResult.IsValid) throw new ValidationException(validationResult.Errors);

        var fromDate = searchDto.FromDate;

        var pagedEvents = await eventRepository.GetFilteredAsync(
            organizerIdFilter,
            userId,
            searchDto.SearchTerm,
            fromDate,
            searchDto.ToDate,
            searchDto.Type,
            sortOrder,
            searchDto.PageNumber,
            searchDto.PageSize,
            cancellationToken
        );

        var eventDtos = pagedEvents.Items.Select(e => new EventDto(
            e.Id,
            e.Name,
            e.Description ?? string.Empty,
            e.Date,
            e.Type,
            e.OrganizerId,
            e.Venue?.Name ?? "TBD",
            e.VenueId,
            e.Venue?.ImageUrl
        )).ToList();

        return new PagedResult<EventDto>(
            eventDtos, pagedEvents.TotalCount, pagedEvents.PageNumber, pagedEvents.PageSize
        );
    }

    public async Task<EventDto?> GetEventByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var e = await eventRepository.GetByIdAsync(id, cancellationToken);
        if (e == null) return null;

        return new EventDto(
            e.Id,
            e.Name,
            e.Description ?? string.Empty,
            e.Date,
            e.Type,
            e.OrganizerId,
            e.Venue?.Name ?? "TBD",
            e.VenueId,
            e.Venue?.ImageUrl
        );
    }

    public async Task<EventDetailsDto?> GetEventDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{EventCacheKeyPrefix}{id}";

        if (cache.TryGetValue(cacheKey, out EventDetailsDto? cachedEvent))
        {
            return cachedEvent;
        }

        var eventEntity = await eventRepository.GetDetailsByIdAsync(id, cancellationToken);
        if (eventEntity == null) return null;

        var userId = httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var isOrganizer = !string.IsNullOrEmpty(userId) && eventEntity.OrganizerId == userId;

        var guestsDto = eventEntity.Guests.Select(g =>
        {
            // Mask data if not organizer
            if (!isOrganizer)
            {
                return new GuestDto(
                    g.Id,
                    g.FirstName,
                    g.LastName,
                    "REDACTED", // Masked Email
                    "",         // Masked CountryCode
                    ""          // Masked Phone
                );
            }

            var (countryCode, localNumber) = ParsePhoneNumber(g.PhoneNumber);

            return new GuestDto(
                g.Id,
                g.FirstName,
                g.LastName,
                g.Email,
                countryCode,
                localNumber
            );
        }).ToList();

        var eventDetails = new EventDetailsDto(
            eventEntity.Venue?.Capacity ?? 0,
            eventEntity.IsPrivate,
            guestsDto,
            eventEntity.Id,
            eventEntity.Name,
            eventEntity.Description ?? string.Empty,
            eventEntity.Date,
            eventEntity.Type,
            eventEntity.OrganizerId,
            eventEntity.Venue?.Name ?? "TBD",
            eventEntity.VenueId,
            eventEntity.Venue?.ImageUrl,
            eventEntity.Venue?.Address
        )
        {
            IsOrganizer = isOrganizer,
            IsJoined = false
        };

        // Cache for shorter duration or based on user role? 
        // CAUTION: Caching masked data might serve it to the organizer if we are not careful with keys.
        // For now, let's disable caching OR make key user-specific if we want to support this.
        // Given the requirement to fix privacy, disabling cache for details is safer 
        // OR we just execute the mapping after retrieving from cache (which stores raw entity? No, it stores DTO).
        // Safest quick fix: Use different cache keys for Organizer vs Public.

        // Simplified Logic: Just don't cache for now to ensure correctness, or cache the "Public" version.
        // Let's rely on the repository cache or assume low traffic for now.
        // But to better follow the user's "Fix" request, I will remove the cache logic for this DTO 
        // to avoid serving masked data to organizer or unmasked to others.

        // var cacheOptions = new MemoryCacheEntryOptions()... cache.Set...

        return eventDetails;
    }

    public async Task<int> CreateEventAsync(string userId, CreateEventDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await createValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid) throw new ValidationException(validationResult.Errors);

        var eventEntity = new Event
        {
            Name = dto.Name,
            Description = dto.Description,
            Date = dto.Date,
            Type = dto.Type,
            VenueId = dto.VenueId == 0 ? null : dto.VenueId,
            OrganizerId = userId,
            IsPrivate = false,
            CreatedAt = DateTime.UtcNow
        };

        return await eventRepository.AddAsync(eventEntity, cancellationToken);
    }

    public async Task UpdateEventAsync(string userId, UpdateEventDto dto, CancellationToken cancellationToken = default)
    {
        var validationResult = await updateValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid) throw new ValidationException(validationResult.Errors);

        var eventEntity = await eventRepository.GetByIdAsync(dto.Id, cancellationToken);
        if (eventEntity == null) throw new KeyNotFoundException($"Event {dto.Id} not found");

        if (eventEntity.OrganizerId != userId) throw new UnauthorizedAccessException("Not your event");

        if (eventEntity.Date < DateTime.Now)
            throw new InvalidOperationException("Cannot edit an event that has already ended.");

        eventEntity.Name = dto.Name;
        eventEntity.Description = dto.Description;
        eventEntity.Date = dto.Date;
        eventEntity.Type = dto.Type;
        eventEntity.VenueId = dto.VenueId == 0 ? null : dto.VenueId;

        await eventRepository.UpdateAsync(eventEntity, cancellationToken);

        InvalidateEventCache(dto.Id);
    }

    public async Task DeleteEventAsync(string userId, int eventId, CancellationToken cancellationToken = default)
    {
        var eventEntity = await eventRepository.GetByIdAsync(eventId, cancellationToken);
        if (eventEntity == null) return;

        if (eventEntity.OrganizerId != userId)
            throw new UnauthorizedAccessException("Not your event");

        await eventRepository.DeleteAsync(eventEntity, cancellationToken);

        InvalidateEventCache(eventId);
    }

    public async Task JoinEventAsync(int eventId, string userId, CancellationToken cancellationToken = default)
    {
        var eventEntity = await eventRepository.GetByIdAsync(eventId, cancellationToken);
        if (eventEntity == null) throw new KeyNotFoundException($"Event {eventId} not found");

        if (eventEntity.Date < DateTime.Now)
            throw new InvalidOperationException("Cannot join an event that has already ended.");

        if (eventEntity.OrganizerId == userId)
            throw new InvalidOperationException("You cannot join your own event as a guest.");

        // Check if already joined (lightweight check)
        var isJoined = await eventRepository.IsUserJoinedAsync(eventId, userId, cancellationToken);
        if (isJoined)
        {
            throw new InvalidOperationException("You are already registered for this event.");
        }

        // Try to join atomically
        var success = await eventRepository.TryJoinEventAsync(eventId, userId, cancellationToken);
        if (!success)
        {
            // If failed, it's likely full or a race condition occurred
            // We can double check capacity for a better error message, but generally:
            throw new InvalidOperationException("Sorry, this event is fully booked or unavailable.");
        }

        InvalidateEventCache(eventId);
    }

    public async Task LeaveEventAsync(int eventId, string userId, CancellationToken cancellationToken = default)
    {
        await eventRepository.RemoveGuestAsync(eventId, userId, cancellationToken);

        InvalidateEventCache(eventId);
    }

    public async Task<bool> IsUserJoinedAsync(int eventId, string userId, CancellationToken cancellationToken = default)
    {
        return await eventRepository.IsUserJoinedAsync(eventId, userId, cancellationToken);
    }

    private void InvalidateEventCache(int eventId)
    {
        cache.Remove($"{EventCacheKeyPrefix}{eventId}");
    }

    private static (string CountryCode, string PhoneNumber) ParsePhoneNumber(string? fullPhoneNumber)
    {
        if (string.IsNullOrEmpty(fullPhoneNumber)) return (CountryConstants.DefaultCode, string.Empty);

        var country = CountryConstants.SupportedCountries
            .OrderByDescending(c => c.Code.Length)
            .FirstOrDefault(c => fullPhoneNumber.StartsWith(c.Code));

        if (country != null)
        {
            var localNumber = fullPhoneNumber.Substring(country.Code.Length);
            return (country.Code, localNumber);
        }

        return (CountryConstants.DefaultCode, fullPhoneNumber);
    }
}