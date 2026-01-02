using EventPlanning.Domain.Entities;
using EventPlanning.Domain.Enums;
using EventPlanning.Domain.Models;

namespace EventPlanning.Domain.Interfaces;

public interface IEventRepository
{
    Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Guid> AddAsync(Event eventEntity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Event eventEntity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Event eventEntity, CancellationToken cancellationToken = default);

    Task<PagedList<Event>> GetFilteredAsync(
        Guid? organizerId,
        Guid? viewerId,
        string? searchTerm,
        DateTime? from,
        DateTime? to,
        EventType? type,
        string? sortOrder,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<Event?> GetDetailsByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> HasEventsAtVenueAsync(Guid venueId, CancellationToken cancellationToken = default);
}