namespace EventPlanning.Domain.Interfaces;

public interface IUserRepository
{
    Task<bool> IsPhoneNumberTakenAsync(string phoneNumber, string userId, CancellationToken cancellationToken);
}