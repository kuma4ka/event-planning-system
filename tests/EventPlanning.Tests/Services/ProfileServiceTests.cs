using EventPlanning.Application.DTOs.Profile;
using EventPlanning.Application.Interfaces;
using EventPlanning.Application.Services;
using EventPlanning.Domain.Entities;
using EventPlanning.Domain.Interfaces;
using EventPlanning.Domain.Enums;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using EventPlanning.Application.Constants;

namespace EventPlanning.Tests.Services;

public class ProfileServiceTests
{
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly Mock<IGuestRepository> _guestRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IValidator<EditProfileDto>> _profileValidatorMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly ProfileService _service;

    public ProfileServiceTests()
    {
        _identityServiceMock = new Mock<IIdentityService>();
        Mock<IEventRepository> eventRepositoryMock = new Mock<IEventRepository>();
        _guestRepositoryMock = new Mock<IGuestRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _profileValidatorMock = new Mock<IValidator<EditProfileDto>>();
        Mock<IValidator<ChangePasswordDto>> passwordValidatorMock = new Mock<IValidator<ChangePasswordDto>>();
        Mock<ICountryService> countryServiceMock = new Mock<ICountryService>();
        _cacheServiceMock = new Mock<ICacheService>();
        Mock<ILogger<ProfileService>> loggerMock = new Mock<ILogger<ProfileService>>();

        _service = new ProfileService(
            _identityServiceMock.Object,
            eventRepositoryMock.Object,
            _guestRepositoryMock.Object,
            _userRepositoryMock.Object,
            _profileValidatorMock.Object,
            passwordValidatorMock.Object,
            countryServiceMock.Object,
            _cacheServiceMock.Object,
            loggerMock.Object
        );
    }

    [Fact]
    public async Task UpdateProfileAsync_ShouldInvalidateCache_WhenGuestsAreUpdated()
    {
        var userId = Guid.CreateVersion7();
        var user = new User(userId.ToString(), "John", "Doe", UserRole.User, "john@example.com", "john@example.com", "5550001", "+1");
        var dto = new EditProfileDto { FirstName = "Johnny", LastName = "Doe", CountryCode = "+1", PhoneNumber = "5551234" };
        var eventId1 = Guid.CreateVersion7();
        var eventId2 = Guid.CreateVersion7();

        _profileValidatorMock.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _userRepositoryMock.Setup(r => r.GetByIdentityIdAsync(userId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _guestRepositoryMock.Setup(r => r.UpdateGuestDetailsByEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Guid> { eventId1, eventId2 });

        _identityServiceMock.Setup(i => i.UpdatePhoneNumberAsync(It.IsAny<Guid>(), It.IsAny<string>()))
            .ReturnsAsync((true, new string[] { }));

        await _service.UpdateProfileAsync(userId, dto);

        _guestRepositoryMock.Verify(r => r.UpdateGuestDetailsByEmailAsync(user.Email!, dto.FirstName, dto.LastName, dto.CountryCode, "+15551234", It.IsAny<CancellationToken>()), Times.Once);
        
        _cacheServiceMock.Verify(c => c.Remove(CacheKeyGenerator.GetEventKeyPublic(eventId1)), Times.Once);
        _cacheServiceMock.Verify(c => c.Remove(CacheKeyGenerator.GetEventKeyOrganizer(eventId1)), Times.Once);
        _cacheServiceMock.Verify(c => c.Remove(CacheKeyGenerator.GetEventKeyPublic(eventId2)), Times.Once);
        _cacheServiceMock.Verify(c => c.Remove(CacheKeyGenerator.GetEventKeyOrganizer(eventId2)), Times.Once);
    }
}
