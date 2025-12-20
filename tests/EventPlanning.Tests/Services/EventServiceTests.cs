using EventPlanning.Application.DTOs;
using EventPlanning.Application.Services;
using EventPlanning.Domain.Entities;
using EventPlanning.Domain.Enums;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace EventPlanning.Tests.Services;

public class EventServiceTests
{
    private readonly Mock<IEventRepository> _eventRepoMock;
    private readonly Mock<IValidator<CreateEventDto>> _createValidatorMock;

    private readonly EventService _service;

    public EventServiceTests()
    {
        _eventRepoMock = new Mock<IEventRepository>();
        _createValidatorMock = new Mock<IValidator<CreateEventDto>>();
        Mock<IValidator<UpdateEventDto>> updateValidatorMock = new Mock<IValidator<UpdateEventDto>>();

        _service = new EventService(
            _eventRepoMock.Object, 
            _createValidatorMock.Object,
            updateValidatorMock.Object
        );
    }

    [Fact]
    public async Task CreateEventAsync_ShouldCallRepository_WhenDtoIsValid()
    {
        // Arrange
        var dto = new CreateEventDto(
            "Test Event", 
            "Description", 
            DateTime.Now.AddDays(1), 
            EventType.Conference, 
            1, // VenueId
            "user-1" // OrganizerId
        );

        _createValidatorMock
            .Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Act
        await _service.CreateEventAsync(dto);

        // Assert
        _eventRepoMock.Verify(x => x.AddAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateEventAsync_ShouldThrowValidationException_WhenDtoIsInvalid()
    {
        // Arrange
        var dto = new CreateEventDto("", "", DateTime.Now, EventType.Conference, null, "");

        var validationFailure = new ValidationResult(new[] { new ValidationFailure("Name", "Required") });
        _createValidatorMock
            .Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationFailure);

        // Act
        Func<Task> act = async () => await _service.CreateEventAsync(dto);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
        
        _eventRepoMock.Verify(x => x.AddAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}