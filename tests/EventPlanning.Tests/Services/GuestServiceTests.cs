using EventPlanning.Application.DTOs.Guest;
using EventPlanning.Application.Services;
using EventPlanning.Domain.Entities;
using EventPlanning.Domain.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using EventPlanning.Domain.Enums;

using EventPlanning.Application.Interfaces;

namespace EventPlanning.Tests.Services;

public class GuestServiceTests
{
    private readonly Mock<IGuestRepository> _guestRepositoryMock;
    private readonly Mock<IEventRepository> _eventRepositoryMock;
    private readonly Mock<IValidator<AddGuestManuallyDto>> _manualAddValidatorMock;
    private readonly Mock<IValidator<UpdateGuestDto>> _updateValidatorMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IMemoryCache> _cacheMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly GuestService _service;

    public GuestServiceTests()
    {
        _guestRepositoryMock = new Mock<IGuestRepository>();
        _eventRepositoryMock = new Mock<IEventRepository>();
        _manualAddValidatorMock = new Mock<IValidator<AddGuestManuallyDto>>();
        _updateValidatorMock = new Mock<IValidator<UpdateGuestDto>>();
        _cacheMock = new Mock<IMemoryCache>();
        _userRepoMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _service = new GuestService(
            _guestRepositoryMock.Object,
            _eventRepositoryMock.Object,
            _manualAddValidatorMock.Object,
            _updateValidatorMock.Object,
            _userRepoMock.Object,
            _cacheMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task UpdateGuestAsync_ShouldThrowInvalidOperationException_WhenGuestIsRegisteredUser()
    {
        var userId = Guid.CreateVersion7();
        var guestId = Guid.CreateVersion7();
        var eventId = Guid.CreateVersion7();

        var organizerUser = new User(userId.ToString(), "Org", "User", UserRole.User, "org@test.com", "org@test.com",
            "1234567890", "+1");

        var dto = new UpdateGuestDto(guestId, eventId, "New", "Name", "new@example.com", "+1", "1234567");
        var eventEntity = new Event("Event", "Desc", DateTime.UtcNow.AddDays(1), EventType.Conference, organizerUser.Id,
            null);

        var guest = new Guest("First", "Last", "old@example.com", eventId, "+1", "0000000",
            Guid.CreateVersion7()); // Registered user

        _updateValidatorMock.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _guestRepositoryMock.Setup(r => r.GetByIdAsync(guestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(guest);

        _userRepoMock.Setup(r => r.GetByIdentityIdAsync(userId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(organizerUser);

        _eventRepositoryMock.Setup(r => r.GetByIdAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(eventEntity);

        _eventRepositoryMock.Setup(r => r.GetByIdAsync(guest.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(eventEntity);


        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateGuestAsync(userId, dto));
    }

    [Fact]
    public async Task UpdateGuestAsync_ShouldSucceed_WhenGuestIsManual()
    {
        var userId = Guid.CreateVersion7();
        var guestId = Guid.CreateVersion7();
        var eventId = Guid.CreateVersion7();

        var organizerUser = new User(userId.ToString(), "Org", "User", UserRole.User, "org@test.com", "org@test.com",
            "1234567890", "+1");

        var dto = new UpdateGuestDto(guestId, eventId, "New", "Name", "new@example.com", "+1", "1234567");
        var eventEntity = new Event("Event", "Desc", DateTime.UtcNow.AddDays(1), EventType.Conference, organizerUser.Id,
            null);

        var guest = new Guest("First", "Last", "old@example.com", eventId, "+1", "0000000", null);

        _updateValidatorMock.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _guestRepositoryMock.Setup(r => r.GetByIdAsync(guestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(guest);

        _userRepoMock.Setup(r => r.GetByIdentityIdAsync(userId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(organizerUser);

        _eventRepositoryMock.Setup(r => r.GetByIdAsync(guest.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(eventEntity);

        _guestRepositoryMock.Setup(r => r.EmailExistsAtEventAsync(It.IsAny<Guid>(), It.IsAny<string>(),
                It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);


        await _service.UpdateGuestAsync(userId, dto);


        _guestRepositoryMock.Verify(r => r.UpdateAsync(guest, It.IsAny<CancellationToken>()), Times.Once);
    }
}