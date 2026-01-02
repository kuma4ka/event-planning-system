namespace EventPlanning.Application.Interfaces;

public interface IEventParticipationService
{
    Task JoinEventAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default);
    Task LeaveEventAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> IsUserJoinedAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default);
}
