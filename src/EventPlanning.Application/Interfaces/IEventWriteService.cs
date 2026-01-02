using EventPlanning.Application.DTOs.Event;

namespace EventPlanning.Application.Interfaces;

public interface IEventWriteService
{
    Task<Guid> CreateEventAsync(Guid userId, CreateEventDto dto, CancellationToken cancellationToken = default);

    Task UpdateEventAsync(Guid userId, UpdateEventDto dto, CancellationToken cancellationToken = default);

    Task DeleteEventAsync(Guid userId, Guid eventId, CancellationToken cancellationToken = default);
}
