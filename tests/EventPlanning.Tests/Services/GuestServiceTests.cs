using EventPlanning.Application.DTOs.Guest;
using EventPlanning.Application.Interfaces;
using EventPlanning.Application.Services;
using EventPlanning.Domain.Entities;
using EventPlanning.Domain.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;

namespace EventPlanning.Tests.Services;

public class GuestServiceTests
{
    private readonly Mock<IGuestRepository> _guestRepositoryMock;
    private readonly Mock<IEventRepository> _eventRepositoryMock;
    private readonly Mock<IValidator<CreateGuestDto>> _createValidatorMock;
    private readonly Mock<IValidator<AddGuestManuallyDto>> _manualAddValidatorMock;
    private readonly Mock<IValidator<UpdateGuestDto>> _updateValidatorMock;
    private readonly Mock<IMemoryCache> _cacheMock;
    private readonly Mock<ILogger<GuestService>> _loggerMock;
    private readonly GuestService _service;

    public GuestServiceTests()
    {
        _guestRepositoryMock = new Mock<IGuestRepository>();
        _eventRepositoryMock = new Mock<IEventRepository>();
        _createValidatorMock = new Mock<IValidator<CreateGuestDto>>();
        _manualAddValidatorMock = new Mock<IValidator<AddGuestManuallyDto>>();
        _updateValidatorMock = new Mock<IValidator<UpdateGuestDto>>();
        _cacheMock = new Mock<IMemoryCache>();
        _loggerMock = new Mock<ILogger<GuestService>>();

        _service = new GuestService(
            _guestRepositoryMock.Object,
            _eventRepositoryMock.Object,
            _createValidatorMock.Object,
            _manualAddValidatorMock.Object,
            _updateValidatorMock.Object,
            _cacheMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task UpdateGuestAsync_ShouldThrowInvalidOperationException_WhenGuestIsRegisteredUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var guestId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var registeredUserId = Guid.NewGuid();

        var dto = new UpdateGuestDto(guestId, eventId, "New", "Name", "new@example.com", "+1", "1234567");

        var eventEntity = new Event("Event", "Desc", DateTime.UtcNow.AddDays(1), Domain.Enums.EventType.Conference, userId, null);
        
        // Use reflection to set Id as setter is private/init only usually or match standard Entity pattern
        // Assuming we can just mock GetByIdAsync to return the Guest

        var guest = new Guest("First", "Last", "old@example.com", eventId, "+1", "0000000", registeredUserId); // Has UserId!
        // We need to ensure Guest.Id matches guestId. 
        // Since Id is created in constructor, we can't easily set it. 
        // We will just assume the repository returns this guest when asked for guestId.

        _updateValidatorMock.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _guestRepositoryMock.Setup(r => r.GetByIdAsync(guestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(guest);
            
        _eventRepositoryMock.Setup(r => r.GetByIdAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(eventEntity); // This mock might be needed if guest.Event is null

        // In GuestService.UpdateGuestAsync: 
        // var eventEntity = guest.Event ?? await eventRepository.GetByIdAsync...
        // Since we created guest without Event object, it will query repository.
        // But first, we need to match EventId.
        // Guest.EventId is set in constructor.
        // So we need to ensure eventEntity.Id matches guest.EventId? 
        // Actually, GuestService uses guest.EventId to fetch event. 
        // But eventEntity.Id is generated inside Event constructor... 
        // This makes testing tricky if we can't set IDs. 
        // Let's rely on eventRepository.GetByIdAsync being called with WHATEVER guest.EventId is.
        
        _eventRepositoryMock.Setup(r => r.GetByIdAsync(guest.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(eventEntity);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateGuestAsync(userId, dto));
    }

    [Fact]
    public async Task UpdateGuestAsync_ShouldSucceed_WhenGuestIsManual()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var guestId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        var dto = new UpdateGuestDto(guestId, eventId, "New", "Name", "new@example.com", "+1", "1234567");
        var eventEntity = new Event("Event", "Desc", DateTime.UtcNow.AddDays(1), Domain.Enums.EventType.Conference, userId, null);

        var guest = new Guest("First", "Last", "old@example.com", eventId, "+1", "0000000", null); // No UserId

        _updateValidatorMock.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _guestRepositoryMock.Setup(r => r.GetByIdAsync(guestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(guest);

        _eventRepositoryMock.Setup(r => r.GetByIdAsync(guest.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(eventEntity);
            
        // Mock Email/Phone checks
        _guestRepositoryMock.Setup(r => r.EmailExistsAtEventAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        await _service.UpdateGuestAsync(userId, dto);

        // Assert
        _guestRepositoryMock.Verify(r => r.UpdateAsync(guest, It.IsAny<CancellationToken>()), Times.Once);
    }
}
