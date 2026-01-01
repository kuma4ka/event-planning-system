namespace EventPlanning.Application.Interfaces;

public interface IEventParticipationService
{
    Task JoinEventAsync(Guid eventId, string userId, CancellationToken cancellationToken = default);
    Task LeaveEventAsync(Guid eventId, string userId, CancellationToken cancellationToken = default);
    Task<bool> IsUserJoinedAsync(Guid eventId, string userId, CancellationToken cancellationToken = default);
}
