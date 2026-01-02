using EventPlanning.Domain.Entities;

namespace EventPlanning.Domain.Interfaces;

public interface IGuestRepository
{
    Task<Guest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Guest guest, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guest guest, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guest guest, CancellationToken cancellationToken = default);

    Task<bool> IsUserJoinedAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default);
    Task RemoveGuestByUserIdAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default);
    Task<int> CountJoinedEventsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<int> CountGuestsAtEventAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAtEventAsync(Guid eventId, string email, Guid? excludeGuestId = null, CancellationToken cancellationToken = default);
    Task<bool> PhoneExistsAtEventAsync(Guid eventId, string phoneNumber, Guid? excludeGuestId = null, CancellationToken cancellationToken = default);
    Task<bool> TryJoinEventAsync(Guest guest, CancellationToken cancellationToken = default);
    Task<List<Guid>> UpdateGuestDetailsByEmailAsync(string email, string firstName, string lastName, string countryCode, string? phoneNumber, CancellationToken cancellationToken = default);
}