using EventPlanning.Application.DTOs.Event;
using EventPlanning.Application.Models;

namespace EventPlanning.Application.Interfaces;

public interface IEventService
{
    Task<PagedResult<EventDto>> GetEventsAsync(
        Guid userId,
        Guid? organizerIdFilter,
        EventSearchDto searchDto,
        string? sortOrder,
        CancellationToken cancellationToken = default);

    Task<EventDto?> GetEventByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<EventDetailsDto?> GetEventDetailsAsync(Guid id, Guid? userId, CancellationToken cancellationToken = default);

    Task<Guid> CreateEventAsync(Guid userId, CreateEventDto dto, CancellationToken cancellationToken = default);

    Task UpdateEventAsync(Guid userId, UpdateEventDto dto, CancellationToken cancellationToken = default);

    Task DeleteEventAsync(Guid userId, Guid eventId, CancellationToken cancellationToken = default);


}