using EventPlanning.Application.DTOs.Event;
using EventPlanning.Application.Services;
using EventPlanning.Domain.Entities;
using EventPlanning.Domain.Enums;
using EventPlanning.Domain.Interfaces;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace EventPlanning.Tests.Services;

public class EventServiceTests
{
    private readonly Mock<IEventRepository> _eventRepoMock;
    private readonly Mock<IValidator<CreateEventDto>> _createValidatorMock;
    private readonly Mock<IValidator<UpdateEventDto>> _updateValidatorMock;
    private readonly Mock<IValidator<EventSearchDto>> _searchValidatorMock;

    private readonly EventService _service;

    public EventServiceTests()
    {
        _eventRepoMock = new Mock<IEventRepository>();
        _createValidatorMock = new Mock<IValidator<CreateEventDto>>();
        _updateValidatorMock = new Mock<IValidator<UpdateEventDto>>();
        _searchValidatorMock = new Mock<IValidator<EventSearchDto>>();

        _service = new EventService(
            _eventRepoMock.Object,
            _createValidatorMock.Object,
            _updateValidatorMock.Object,
            _searchValidatorMock.Object
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

        var validationFailure = new ValidationResult(new[] { new ValidationFailure("Name", "Required") });

        _createValidatorMock
            .Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationFailure);

        // Act
        Func<Task> act = async () => await _service.CreateEventAsync(userId, dto);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();

        _eventRepoMock.Verify(x => x.AddAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}