using EventPlanning.Application.DTOs.Event;
using EventPlanning.Application.Models;

namespace EventPlanning.Application.Interfaces;

public interface IEventReadService
{
    Task<PagedResult<EventDto>> GetEventsAsync(
        Guid userId,
        Guid? organizerIdFilter,
        EventSearchDto searchDto,
        string? sortOrder,
        CancellationToken cancellationToken = default);

    Task<EventDto?> GetEventByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<EventDetailsDto?> GetEventDetailsAsync(Guid id, Guid? userId, CancellationToken cancellationToken = default);

    Task<EventDto> GetEventForEditAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default);
}
