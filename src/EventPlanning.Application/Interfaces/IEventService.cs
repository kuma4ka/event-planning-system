using EventPlanning.Application.DTOs;
using EventPlanning.Application.Models;

namespace EventPlanning.Application.Interfaces;

public interface IEventService
{
    Task<PagedResult<EventDto>> GetEventsAsync(string userId, EventSearchDto searchDto, CancellationToken cancellationToken = default);

    Task<EventDto?> GetEventByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<EventDetailsDto?> GetEventDetailsAsync(int id, CancellationToken cancellationToken = default);

    Task CreateEventAsync(CreateEventDto createEventDto, CancellationToken cancellationToken = default);
    
    Task UpdateEventAsync(string userId, UpdateEventDto dto, CancellationToken cancellationToken = default);
    
    Task DeleteEventAsync(string userId, int eventId, CancellationToken cancellationToken = default);
}