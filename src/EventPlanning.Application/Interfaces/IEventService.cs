using EventPlanning.Application.DTOs.Event;
using EventPlanning.Application.Models;

namespace EventPlanning.Application.Interfaces;

public interface IEventService
{
    Task<PagedResult<EventDto>> GetEventsAsync(
        string userId,
        string? organizerIdFilter,
        EventSearchDto searchDto,
        string? sortOrder,
        CancellationToken cancellationToken = default);

    Task<EventDto?> GetEventByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<EventDetailsDto?> GetEventDetailsAsync(Guid id, string? userId, CancellationToken cancellationToken = default);

    Task<Guid> CreateEventAsync(string userId, CreateEventDto dto, CancellationToken cancellationToken = default);

    Task UpdateEventAsync(string userId, UpdateEventDto dto, CancellationToken cancellationToken = default);

    Task DeleteEventAsync(string userId, Guid eventId, CancellationToken cancellationToken = default);


}