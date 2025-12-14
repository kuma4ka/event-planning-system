using EventPlanning.Application.DTOs;

namespace EventPlanning.Application.Interfaces;

public interface IEventService
{
    Task<List<EventDto>> GetAllEventsAsync(CancellationToken cancellationToken = default);
    Task<EventDto?> GetEventByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<EventDto>> GetEventsByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task CreateEventAsync(CreateEventDto createEventDto, CancellationToken cancellationToken = default);
    Task UpdateEventAsync(string userId, UpdateEventDto dto, CancellationToken cancellationToken = default);
    Task DeleteEventAsync(string userId, int eventId, CancellationToken cancellationToken = default);
}