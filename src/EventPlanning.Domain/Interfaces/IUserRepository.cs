using EventPlanning.Domain.Entities;

namespace EventPlanning.Domain.Interfaces;

public interface IUserRepository
{
    Task<bool> IsPhoneNumberTakenAsync(string phoneNumber, string userId, CancellationToken cancellationToken);
    Task<User?> GetByIdAsync(string userId, CancellationToken cancellationToken);
}