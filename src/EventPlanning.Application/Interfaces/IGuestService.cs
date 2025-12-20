using EventPlanning.Application.DTOs;

namespace EventPlanning.Application.Interfaces;

public interface IGuestService
{
    Task AddGuestAsync(string userId, CreateGuestDto dto, CancellationToken cancellationToken = default);

    Task AddGuestManuallyAsync(string currentUserId, AddGuestManuallyDto dto,
        CancellationToken cancellationToken = default);

    Task RemoveGuestAsync(string userId, string guestId, CancellationToken cancellationToken = default);
}