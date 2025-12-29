using EventPlanning.Application.DTOs.Event;
using EventPlanning.Application.Services;
using EventPlanning.Domain.Entities;
using EventPlanning.Domain.Enums;
using EventPlanning.Domain.Interfaces;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace EventPlanning.Tests.Services;

public class EventServiceTests
{
    private readonly Mock<IEventRepository> _eventRepoMock;
    private readonly Mock<IValidator<CreateEventDto>> _createValidatorMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;

    private readonly EventService _service;

    public EventServiceTests()
    {
        _eventRepoMock = new Mock<IEventRepository>();
        _createValidatorMock = new Mock<IValidator<CreateEventDto>>();
        Mock<IValidator<UpdateEventDto>> updateValidatorMock = new Mock<IValidator<UpdateEventDto>>();
        Mock<IValidator<EventSearchDto>> searchValidatorMock = new Mock<IValidator<EventSearchDto>>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        IMemoryCache cache = new MemoryCache(new MemoryCacheOptions());

        _service = new EventService(
            _eventRepoMock.Object,
            _createValidatorMock.Object,
            updateValidatorMock.Object,
            searchValidatorMock.Object,
            _httpContextAccessorMock.Object,
            cache
        );
    }

    [Fact]
    public async Task CreateEventAsync_ShouldCallRepository_AndReturnId_WhenDtoIsValid()
    {
        // Arrange
        var userId = "user-1";
        var expectedEventId = 123;

        var dto = new CreateEventDto(
            "Test Event",
            "Description",
            DateTime.UtcNow.AddDays(5),
            EventType.Conference,
            1
        );

        _createValidatorMock
            .Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _eventRepoMock
            .Setup(x => x.AddAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedEventId);

        // Act
        var result = await _service.CreateEventAsync(userId, dto);

        // Assert
        result.Should().Be(expectedEventId);

        _eventRepoMock.Verify(x => x.AddAsync(It.Is<Event>(e =>
            e.Name == dto.Name &&
            e.OrganizerId == userId &&
            e.IsPrivate == false &&
            e.Type == dto.Type &&
            e.VenueId == dto.VenueId
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateEventAsync_ShouldThrowValidationException_WhenDtoIsInvalid()
    {
        // Arrange
        var userId = "user-1";
        var dto = new CreateEventDto("", "", DateTime.UtcNow, EventType.Conference, 0);

        var validationFailure = new ValidationResult([new ValidationFailure("Name", "Required")]);

        _createValidatorMock
            .Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationFailure);

        // Act
        Func<Task> act = async () => await _service.CreateEventAsync(userId, dto);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();

        _eventRepoMock.Verify(x => x.AddAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task JoinEventAsync_ShouldThrowInvalidOperation_WhenUserAlreadyJoined()
    {
        // Arrange
        var eventId = 1;
        var userId = "user-123";

        var eventEntity = new Event(
            "Test Event",
            "Description",
            DateTime.UtcNow.AddDays(5),
            EventType.Conference,
            "other-organizer");

        // Reflection to set Id for testing
        typeof(Event).GetProperty(nameof(Event.Id))!.SetValue(eventEntity, eventId);

        _eventRepoMock
            .Setup(r => r.GetByIdAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(eventEntity);

        _eventRepoMock
            .Setup(r => r.IsUserJoinedAsync(eventId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await _service.JoinEventAsync(eventId, userId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("You are already registered for this event.");

        _eventRepoMock.Verify(x => x.TryJoinEventAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetEventDetailsAsync_ShouldMaskData_WhenUserIsNotOrganizer()
    {
        // Arrange
        var eventId = 1;
        var organizerId = "user-1";
        var currentUserId = "user-2";

        var guest = new Guest("g1", "Test", "Guest", "guest@test.com", eventId, "1234567890");

        var eventEntity = new Event(
            "Test Event",
            "Desc",
            DateTime.UtcNow.AddDays(1),
            EventType.Conference,
            organizerId);
        typeof(Event).GetProperty(nameof(Event.Id))!.SetValue(eventEntity, eventId);
        eventEntity.AddGuest(guest);

        _eventRepoMock
            .Setup(r => r.GetDetailsByIdAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(eventEntity);

        SetupHttpContextUser(currentUserId);

        // Act
        var result = await _service.GetEventDetailsAsync(eventId);

        // Assert
        result.Should().NotBeNull();
        result!.Guests.Should().ContainSingle();
        var guestDto = result.Guests.First();

        guestDto.Email.Should().Be("REDACTED");
        guestDto.PhoneNumber.Should().BeEmpty();
        guestDto.CountryCode.Should().BeEmpty();
        guestDto.FirstName.Should().Be("Test"); // Name should remain visible
    }

    [Fact]
    public async Task GetEventDetailsAsync_ShouldShowData_WhenUserIsOrganizer()
    {
        // Arrange
        var eventId = 1;
        var organizerId = "user-1";
        var currentUserId = "user-1"; // Is Organizer

        var guest = new Guest("g1", "Test", "Guest", "guest@test.com", eventId, "+1234567890");

        var eventEntity = new Event(
            "Test Event",
            "Desc",
            DateTime.UtcNow.AddDays(1),
            EventType.Conference,
            organizerId);
        typeof(Event).GetProperty(nameof(Event.Id))!.SetValue(eventEntity, eventId);
        eventEntity.AddGuest(guest);

        _eventRepoMock
            .Setup(r => r.GetDetailsByIdAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(eventEntity);

        SetupHttpContextUser(currentUserId);

        // Act
        var result = await _service.GetEventDetailsAsync(eventId);

        // Assert
        result.Should().NotBeNull();
        var guestDto = result.Guests.First();

        guestDto.Email.Should().Be("guest@test.com");
        guestDto.PhoneNumber.Should().Be("234567890"); // Parsed local number
        guestDto.CountryCode.Should().Be("+1"); // Parsed Code
    }

    private void SetupHttpContextUser(string userId)
    {
        var claims = new List<System.Security.Claims.Claim>
        {
            new(System.Security.Claims.ClaimTypes.NameIdentifier, userId)
        };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext { User = claimsPrincipal };
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
    }
}