using EventPlanning.Application.DTOs.Event;
using EventPlanning.Application.Interfaces;
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
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;

    private readonly EventService _service;

    public EventServiceTests()
    {
        _eventRepoMock = new Mock<IEventRepository>();
        _createValidatorMock = new Mock<IValidator<CreateEventDto>>();
        Mock<IValidator<UpdateEventDto>> updateValidatorMock = new Mock<IValidator<UpdateEventDto>>();
        Mock<IValidator<EventSearchDto>> searchValidatorMock = new Mock<IValidator<EventSearchDto>>();
        _identityServiceMock = new Mock<IIdentityService>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        Mock<IMemoryCache> cacheMock = new Mock<IMemoryCache>();

        _service = new EventService(
            _eventRepoMock.Object,
            _createValidatorMock.Object,
            updateValidatorMock.Object,
            searchValidatorMock.Object,
            _identityServiceMock.Object,
            _httpContextAccessorMock.Object,
            cacheMock.Object
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
            DateTime.Now.AddDays(1),
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
        var dto = new CreateEventDto("", "", DateTime.Now, EventType.Conference, 0);

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
        var userEmail = "duplicate@test.com";

        var existingGuest = new Guest
        {
            Email = userEmail,
            FirstName = "Existing",
            LastName = "Guest"
        };

        var eventEntity = new Event
        {
            Id = eventId,
            OrganizerId = "other-organizer",
            Date = DateTime.Now.AddDays(5),
            Guests = new List<Guest> { existingGuest }
        };

        var user = new User
        {
            Id = userId,
            Email = userEmail,
            FirstName = "Test",
            LastName = "User"
        };

        _eventRepoMock
            .Setup(r => r.GetByIdAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(eventEntity);

        _identityServiceMock
            .Setup(s => s.GetUserByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        Func<Task> act = async () => await _service.JoinEventAsync(eventId, userId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("You are already registered for this event.");

        _eventRepoMock.Verify(x => x.AddGuestAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}