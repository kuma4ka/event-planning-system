namespace EventPlanning.Application.Interfaces;

public interface INewsletterService
{
    Task SubscribeAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ConfirmSubscriptionAsync(string email, string token, CancellationToken cancellationToken = default);
}
