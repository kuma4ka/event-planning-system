namespace EventPlanning.Application.Interfaces;

public interface INewsletterService
{
    Task SubscribeAsync(string email, CancellationToken cancellationToken = default);
}
