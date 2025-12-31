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

    Task<bool> IsUserJoinedAsync(Guid eventId, string userId, CancellationToken cancellationToken = default);
    Task AddGuestAsync(Guest guest, CancellationToken cancellationToken = default);
    Task RemoveGuestAsync(Guid eventId, string userId, CancellationToken cancellationToken = default);
    Task<int> CountJoinedEventsAsync(string userId, CancellationToken cancellationToken = default);

    // Performance optimizations
    Task<Event?> GetDetailsByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> CountGuestsAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task<bool> GuestEmailExistsAsync(Guid eventId, string email, Guid? excludeGuestId = null, CancellationToken cancellationToken = default);
    Task<bool> GuestPhoneExistsAsync(Guid eventId, string phoneNumber, Guid? excludeGuestId = null, CancellationToken cancellationToken = default);
}