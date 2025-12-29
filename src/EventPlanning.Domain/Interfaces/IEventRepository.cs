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
    Task<bool> TryJoinEventAsync(int eventId, string userId, CancellationToken cancellationToken = default);
    Task RemoveGuestAsync(int eventId, string userId, CancellationToken cancellationToken = default);
    Task<int> CountJoinedEventsAsync(string userId, CancellationToken cancellationToken = default);

    // Performance optimizations
    Task<Event?> GetDetailsByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CountGuestsAsync(int eventId, CancellationToken cancellationToken = default);
    Task<bool> GuestEmailExistsAsync(int eventId, string email, string? excludeGuestId = null, CancellationToken cancellationToken = default);
    Task<bool> GuestPhoneExistsAsync(int eventId, string phoneNumber, string? excludeGuestId = null, CancellationToken cancellationToken = default);
}