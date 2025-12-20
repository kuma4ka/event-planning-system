using EventPlanning.Domain.Entities;
using EventPlanning.Domain.Enums;
using EventPlanning.Domain.Models;

namespace EventPlanning.Domain.Interfaces;

public interface IEventRepository
{
    Task<Event?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> AddAsync(Event eventEntity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Event eventEntity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Event eventEntity, CancellationToken cancellationToken = default);

    Task<PagedList<Event>> GetFilteredAsync(
        string? organizerId,
        string? viewerId,
        string? searchTerm,
        DateTime? from,
        DateTime? to,
        EventType? type,
        string? sortOrder,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<bool> IsUserJoinedAsync(int eventId, string userId, CancellationToken cancellationToken = default);
    Task AddGuestAsync(int eventId, string userId, CancellationToken cancellationToken = default);
    Task RemoveGuestAsync(int eventId, string userId, CancellationToken cancellationToken = default);
}