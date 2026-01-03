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
using Mapster;

namespace EventPlanning.Tests.Services;

public class EventServiceTests
{
    private readonly Mock<IEventRepository> _eventRepoMock;
    private readonly Mock<IGuestRepository> _guestRepoMock;
    private readonly Mock<IValidator<CreateEventDto>> _createValidatorMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<ILogger<EventService>> _loggerMock;
    private readonly Mock<ICountryService> _countryServiceMock;

    private readonly EventService _service;

    public EventServiceTests()
    {
        _eventRepoMock = new Mock<IEventRepository>();
        _guestRepoMock = new Mock<IGuestRepository>();
        _createValidatorMock = new Mock<IValidator<CreateEventDto>>();
        _userRepoMock = new Mock<IUserRepository>();
        var updateValidatorMock = new Mock<IValidator<UpdateEventDto>>();
        var searchValidatorMock = new Mock<IValidator<EventSearchDto>>();
        _loggerMock = new Mock<ILogger<EventService>>();
        _countryServiceMock = new Mock<ICountryService>();
        _countryServiceMock.Setup(c => c.ParsePhoneNumber(It.IsAny<string>())).Returns(("+1", "1231231234"));

        _service = new EventService(
            _eventRepoMock.Object,
            _guestRepoMock.Object,
            _createValidatorMock.Object,
            updateValidatorMock.Object,
            searchValidatorMock.Object,
            _userRepoMock.Object,
            _countryServiceMock.Object,
            _loggerMock.Object
        );


        new Application.Mappings.EventMappingConfig().Register(TypeAdapterConfig.GlobalSettings);
    }

    [Fact]
    public async Task CreateEventAsync_ShouldCallRepository_AndReturnId_WhenDtoIsValid()
    {
        var userId = Guid.CreateVersion7();
        var expectedEventId = Guid.CreateVersion7();
        var user = new User(userId.ToString(), "John", "Doe", UserRole.User, "john@example.com", "john@example.com",
            "+123456789", "+1");

        var dto = new CreateEventDto(
            "Test Event",
            "Description",
            DateTime.UtcNow.AddDays(5),
            EventType.Conference,
            Guid.CreateVersion7()
        );

        _createValidatorMock
            .Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _eventRepoMock
            .Setup(x => x.AddAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedEventId);

        _userRepoMock.Setup(x => x.GetByIdentityIdAsync(userId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);


        var result = await _service.CreateEventAsync(userId, dto);


        result.Should().Be(expectedEventId);

        _eventRepoMock.Verify(x => x.AddAsync(It.Is<Event>(e =>
            e.Name == dto.Name &&
            e.IsPrivate == false &&
            e.Type == dto.Type &&
            e.VenueId == dto.VenueId
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateEventAsync_ShouldThrowValidationException_WhenDtoIsInvalid()
    {
        var userId = Guid.CreateVersion7();
        var dto = new CreateEventDto("", "", DateTime.UtcNow, EventType.Conference, Guid.Empty);

        var validationFailure = new ValidationResult([new ValidationFailure("Name", "Required")]);

        _createValidatorMock
            .Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationFailure);

        _userRepoMock.Setup(x => x.GetByIdentityIdAsync(userId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);


        Func<Task> act = async () => await _service.CreateEventAsync(userId, dto);


        await act.Should().ThrowAsync<ValidationException>();

        _eventRepoMock.Verify(x => x.AddAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()), Times.Never);
    }


    [Fact]
    public async Task GetEventDetailsAsync_ShouldMaskData_WhenUserIsNotOrganizer()
    {
        var eventId = Guid.CreateVersion7();
        var organizerId = Guid.CreateVersion7();
        var currentUserId = Guid.CreateVersion7();

        var organizerUser = new User(organizerId.ToString(), "Org", "Anizer", UserRole.User, "org@test.com",
            "org@test.com", "+1234567890", "+1");

        _userRepoMock.Setup(x => x.GetByIdentityIdAsync(currentUserId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User(currentUserId.ToString(), "Viewer", "User", UserRole.User, "viewer@test.com",
                "viewer@test.com", "+1987654321", "+1"));

        var guest = new Guest("Test", "Guest", "guest@test.com", eventId, "+1", "+1234567890");

        var eventEntity = new Event(
            "Test Event",
            "Desc",
            DateTime.UtcNow.AddDays(1),
            EventType.Conference,
            organizerUser.Id);

        typeof(Event).GetProperty(nameof(Event.Id))!.SetValue(eventEntity, eventId);
        eventEntity.AddGuest(guest);

        _eventRepoMock
            .Setup(r => r.GetDetailsByIdAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(eventEntity);


        var result = await _service.GetEventDetailsAsync(eventId, currentUserId);


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
        var eventId = Guid.CreateVersion7();
        var organizerId = Guid.CreateVersion7();
        Guid? currentUserId = organizerId; // Is Organizer

        var organizerUser = new User(organizerId.ToString(), "John", "Doe", UserRole.User, "john@example.com",
            "john@example.com", "+123456789", "+1");
        _userRepoMock.Setup(x => x.GetByIdentityIdAsync(organizerId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(organizerUser);

        var guest = new Guest("Test", "Guest", "guest@test.com", eventId, "+1", "+1234567890");

        var eventEntity = new Event(
            "Test Event",
            "Desc",
            DateTime.UtcNow.AddDays(1),
            EventType.Conference,
            organizerUser.Id);

        typeof(Event).GetProperty(nameof(Event.Id))!.SetValue(eventEntity, eventId);
        eventEntity.AddGuest(guest);

        _eventRepoMock
            .Setup(r => r.GetDetailsByIdAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(eventEntity);

        _countryServiceMock
            .Setup(c => c.ParsePhoneNumber(It.IsAny<string>()))
            .Returns(("+1", "1234567890"));


        var result = await _service.GetEventDetailsAsync(eventId, currentUserId);


        result.Should().NotBeNull();
        var guestDto = result.Guests.First();

        guestDto.Email.Should().Be("guest@test.com");
        guestDto.PhoneNumber.Should().Be("1234567890"); // Parsed local number
        guestDto.CountryCode.Should().Be("+1"); // Parsed Code
    }

    [Fact]
    public async Task GetEventDetailsAsync_ShouldPopulateOrganizerInfo_WhenUserFound()
    {
        var eventId = Guid.CreateVersion7();
        var organizerId = Guid.CreateVersion7();
        var user = new User(organizerId.ToString(), "John", "Doe", UserRole.User, "john@example.com",
            "john@example.com", "+123456789", "+1");

        var eventEntity = new Event(
            "Test Event",
            "Desc",
            DateTime.UtcNow.AddDays(1),
            EventType.Conference,
            user.Id);

        typeof(Event).GetProperty(nameof(Event.Id))!.SetValue(eventEntity, eventId);

        _eventRepoMock
            .Setup(r => r.GetDetailsByIdAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(eventEntity);

        _userRepoMock
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);


        var result = await _service.GetEventDetailsAsync(eventId, (Guid?)null);


        result.Should().NotBeNull();
        result!.OrganizerName.Should().Be("John Doe");
        result.OrganizerEmail.Should().Be("john@example.com");
    }
}