using EventPlanning.Application.DTOs.Event;
using EventPlanning.Application.Services;
using EventPlanning.Domain.Entities;
using EventPlanning.Domain.Enums;
using EventPlanning.Domain.Interfaces;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using EventPlanning.Application.Interfaces;

namespace EventPlanning.Tests.Services;

public class EventServiceTests
{
    private readonly Mock<IEventRepository> _eventRepoMock;
    private readonly Mock<IValidator<CreateEventDto>> _createValidatorMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<ILogger<EventService>> _loggerMock;
    private readonly Mock<ICountryService> _countryServiceMock;

    private readonly EventService _service;

    public EventServiceTests()
    {
        _eventRepoMock = new Mock<IEventRepository>();
        _createValidatorMock = new Mock<IValidator<CreateEventDto>>();
        _userRepoMock = new Mock<IUserRepository>();
        Mock<IValidator<UpdateEventDto>> updateValidatorMock = new Mock<IValidator<UpdateEventDto>>();
        Mock<IValidator<EventSearchDto>> searchValidatorMock = new Mock<IValidator<EventSearchDto>>();
        _loggerMock = new Mock<ILogger<EventService>>();
        _countryServiceMock = new Mock<ICountryService>();
        _countryServiceMock.Setup(c => c.ParsePhoneNumber(It.IsAny<string>())).Returns(("+1", "1231231234"));

        _service = new EventService(
            _eventRepoMock.Object,
            _createValidatorMock.Object,
            updateValidatorMock.Object,
            searchValidatorMock.Object,
            _userRepoMock.Object,
            _countryServiceMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task CreateEventAsync_ShouldCallRepository_AndReturnId_WhenDtoIsValid()
    {
        // Arrange
        var userId = "user-1";
        var expectedEventId = Guid.NewGuid();

        var dto = new CreateEventDto(
            "Test Event",
            "Description",
            DateTime.UtcNow.AddDays(5),
            EventType.Conference,
            Guid.NewGuid()
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
        var dto = new CreateEventDto("", "", DateTime.UtcNow, EventType.Conference, Guid.Empty);

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
    public async Task GetEventDetailsAsync_ShouldMaskData_WhenUserIsNotOrganizer()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var organizerId = "user-1";
        var currentUserId = "user-2";

        var guest = new Guest("Test", "Guest", "guest@test.com", eventId, "+1", "+1234567890");



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

        // Act
        var result = await _service.GetEventDetailsAsync(eventId, currentUserId);

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
        var eventId = Guid.NewGuid();
        var organizerId = "user-1";
        var currentUserId = "user-1"; // Is Organizer

        var guest = new Guest("Test", "Guest", "guest@test.com", eventId, "+1", "+1234567890");

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

        _countryServiceMock
            .Setup(c => c.ParsePhoneNumber(It.IsAny<string>()))
            .Returns(("+1", "234567890"));

        // Act
        var result = await _service.GetEventDetailsAsync(eventId, currentUserId);

        // Assert
        result.Should().NotBeNull();
        var guestDto = result.Guests.First();

        guestDto.Email.Should().Be("guest@test.com");
        guestDto.PhoneNumber.Should().Be("234567890"); // Parsed local number
        guestDto.CountryCode.Should().Be("+1"); // Parsed Code
    }

    [Fact]
    public async Task GetEventDetailsAsync_ShouldPopulateOrganizerInfo_WhenUserFound()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var organizerId = "user-1";
        var user = new User(organizerId, "John", "Doe", UserRole.User, "john@example.com", "john@example.com", "+123456789", "+1");

        var eventEntity = new Event(
            "Test Event",
            "Desc",
            DateTime.UtcNow.AddDays(1),
            EventType.Conference,
            organizerId);
        typeof(Event).GetProperty(nameof(Event.Id))!.SetValue(eventEntity, eventId);

        _eventRepoMock
            .Setup(r => r.GetDetailsByIdAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(eventEntity);

        _userRepoMock
            .Setup(r => r.GetByIdAsync(organizerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _service.GetEventDetailsAsync(eventId, null);

        // Assert
        result.Should().NotBeNull();
        result!.OrganizerName.Should().Be("John Doe");
        result.OrganizerEmail.Should().Be("john@example.com");
    }


}