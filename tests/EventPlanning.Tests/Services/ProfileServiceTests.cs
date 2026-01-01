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

namespace EventPlanning.Tests.Services;

public class ProfileServiceTests
{
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly Mock<IEventRepository> _eventRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IValidator<EditProfileDto>> _profileValidatorMock;
    private readonly Mock<IValidator<ChangePasswordDto>> _passwordValidatorMock;
    private readonly Mock<ICountryService> _countryServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<ILogger<ProfileService>> _loggerMock;
    private readonly ProfileService _service;

    public ProfileServiceTests()
    {
        _identityServiceMock = new Mock<IIdentityService>();
        _eventRepositoryMock = new Mock<IEventRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _profileValidatorMock = new Mock<IValidator<EditProfileDto>>();
        _passwordValidatorMock = new Mock<IValidator<ChangePasswordDto>>();
        _countryServiceMock = new Mock<ICountryService>();
        _cacheServiceMock = new Mock<ICacheService>();
        _loggerMock = new Mock<ILogger<ProfileService>>();

        _service = new ProfileService(
            _identityServiceMock.Object,
            _eventRepositoryMock.Object,
            _userRepositoryMock.Object,
            _profileValidatorMock.Object,
            _passwordValidatorMock.Object,
            _countryServiceMock.Object,
            _cacheServiceMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task UpdateProfileAsync_ShouldInvalidateCache_WhenGuestsAreUpdated()
    {
        // Arrange

        var userId = "user1";
        var user = new User(userId, "John", "Doe", UserRole.User, "john@example.com", "john@example.com", "5550001", "+1");
        var dto = new EditProfileDto { FirstName = "Johnny", LastName = "Doe", CountryCode = "+1", PhoneNumber = "5551234" };
        var eventId1 = Guid.NewGuid();
        var eventId2 = Guid.NewGuid();

        _profileValidatorMock.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _eventRepositoryMock.Setup(r => r.UpdateGuestDetailsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Guid> { eventId1, eventId2 });

        _identityServiceMock.Setup(i => i.UpdatePhoneNumberAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((true, new string[] { }));

        // Act
        await _service.UpdateProfileAsync(userId, dto);

        // Assert
        _eventRepositoryMock.Verify(r => r.UpdateGuestDetailsAsync(user.Email!, dto.FirstName, dto.LastName, dto.CountryCode, "+15551234", It.IsAny<CancellationToken>()), Times.Once);
        
        // Verify cache invalidation
        _cacheServiceMock.Verify(c => c.Remove($"{CachedEventService.EventCacheKeyPrefix}{eventId1}_public"), Times.Once);
        _cacheServiceMock.Verify(c => c.Remove($"{CachedEventService.EventCacheKeyPrefix}{eventId1}_organizer"), Times.Once);
        _cacheServiceMock.Verify(c => c.Remove($"{CachedEventService.EventCacheKeyPrefix}{eventId2}_public"), Times.Once);
        _cacheServiceMock.Verify(c => c.Remove($"{CachedEventService.EventCacheKeyPrefix}{eventId2}_organizer"), Times.Once);
    }
}
