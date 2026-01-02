using EventPlanning.Application.DTOs.Guest;

namespace EventPlanning.Application.Interfaces;

public interface IGuestService
{
    Task AddGuestAsync(Guid userId, CreateGuestDto dto, CancellationToken cancellationToken = default);

    Task AddGuestManuallyAsync(Guid currentUserId, AddGuestManuallyDto dto,
        CancellationToken cancellationToken = default);

    Task UpdateGuestAsync(Guid currentUserId, UpdateGuestDto dto, CancellationToken cancellationToken = default);
    Task RemoveGuestAsync(Guid userId, Guid guestId, CancellationToken cancellationToken = default);
}