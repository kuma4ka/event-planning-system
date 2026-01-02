using EventPlanning.Application.Interfaces;
using EventPlanning.Application.Services;
using EventPlanning.Domain.Entities;
using EventPlanning.Domain.Enums;
using EventPlanning.Domain.Interfaces;
using EventPlanning.Application.Constants;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace EventPlanning.Tests.Services;

public class EventParticipationServiceTests
{
    private readonly Mock<IEventRepository> _eventRepoMock;
    private readonly Mock<IGuestRepository> _guestRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<ILogger<EventParticipationService>> _loggerMock;
    private readonly EventParticipationService _service;

    public EventParticipationServiceTests()
    {
        _eventRepoMock = new Mock<IEventRepository>();
        _guestRepoMock = new Mock<IGuestRepository>();
        _userRepoMock = new Mock<IUserRepository>();
        _cacheServiceMock = new Mock<ICacheService>();
        _loggerMock = new Mock<ILogger<EventParticipationService>>();

        _service = new EventParticipationService(
            _eventRepoMock.Object,
            _guestRepoMock.Object,
            _userRepoMock.Object,
            _cacheServiceMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task JoinEventAsync_ShouldThrowInvalidOperation_WhenUserAlreadyJoined()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var eventEntity = new Event(
            "Test Event",
            "Description",
            DateTime.UtcNow.AddDays(5),
            EventType.Conference,
            Guid.NewGuid(),
            null);

        typeof(Event).GetProperty(nameof(Event.Id))!.SetValue(eventEntity, eventId);

        _eventRepoMock
            .Setup(r => r.GetByIdAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(eventEntity);



        _guestRepoMock
            .Setup(r => r.EmailExistsAtEventAsync(eventId, "test@test.com", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _userRepoMock
            .Setup(r => r.GetByIdentityIdAsync(userId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User(userId.ToString(), "Test", "User", UserRole.User, "test@test.com", "test@test.com", "+123456789", "+1"));

        // Act
        Func<Task> act = async () => await _service.JoinEventAsync(eventId, userId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("You are already registered for this event.");

        _cacheServiceMock.Verify(x => x.Remove(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task JoinEventAsync_ShouldSucceed_WhenUserNotJoined()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var eventEntity = new Event(
            "Test Event",
            "Description",
            DateTime.UtcNow.AddDays(5),
            EventType.Conference,
            Guid.NewGuid(),
            null);
        typeof(Event).GetProperty(nameof(Event.Id))!.SetValue(eventEntity, eventId);

        _eventRepoMock.Setup(r => r.GetByIdAsync(eventId, It.IsAny<CancellationToken>())).ReturnsAsync(eventEntity);
        _guestRepoMock.Setup(r => r.EmailExistsAtEventAsync(eventId, "test@test.com", null, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _guestRepoMock.Setup(r => r.PhoneExistsAtEventAsync(eventId, It.IsAny<string>(), null, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _guestRepoMock.Setup(r => r.TryJoinEventAsync(It.IsAny<Guest>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        
        _userRepoMock.Setup(r => r.GetByIdentityIdAsync(userId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User(userId.ToString(), "Test", "User", UserRole.User, "test@test.com", "test@test.com", "+123456789", "+1"));


        // Act
        await _service.JoinEventAsync(eventId, userId);

        // Assert
        _guestRepoMock.Verify(x => x.TryJoinEventAsync(It.Is<Guest>(g => g.Email.Value == "test@test.com" && g.UserId != userId), It.IsAny<CancellationToken>()), Times.Once); // UserId should be DomainId (Random), not IdentityId
        
        _cacheServiceMock.Verify(x => x.Remove(CacheKeyGenerator.GetEventKeyPublic(eventId)), Times.Once);
        _cacheServiceMock.Verify(x => x.Remove(CacheKeyGenerator.GetEventKeyOrganizer(eventId)), Times.Once);
    }
}
