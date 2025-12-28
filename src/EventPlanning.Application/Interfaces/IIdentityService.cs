using EventPlanning.Domain.Entities;

namespace EventPlanning.Application.Interfaces;

public interface IIdentityService
{
    Task<User?> GetUserByIdAsync(string userId);
}