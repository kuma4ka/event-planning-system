using EventPlanning.Application.DTOs.Venue;
using EventPlanning.Application.Interfaces;
using EventPlanning.Application.Services;
using EventPlanning.Domain.Entities;
using EventPlanning.Domain.Enums;
using EventPlanning.Domain.Interfaces;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace EventPlanning.Tests.Services;

public class VenueServiceTests
{
    private readonly Mock<IVenueRepository> _venueRepoMock;
    private readonly Mock<IEventRepository> _eventRepoMock;
    private readonly Mock<IValidator<CreateVenueDto>> _createValidatorMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly VenueService _service;

    public VenueServiceTests()
    {
        _venueRepoMock = new Mock<IVenueRepository>();
        _eventRepoMock = new Mock<IEventRepository>();
        Mock<IImageService> imageServiceMock = new Mock<IImageService>();
        _createValidatorMock = new Mock<IValidator<CreateVenueDto>>();
        Mock<IValidator<UpdateVenueDto>> updateValidatorMock = new Mock<IValidator<UpdateVenueDto>>();
        _userRepoMock = new Mock<IUserRepository>();
        var loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<VenueService>>();

        _service = new VenueService(
            _venueRepoMock.Object,
            _eventRepoMock.Object,
            imageServiceMock.Object,
            _createValidatorMock.Object,
            updateValidatorMock.Object,
            _userRepoMock.Object,
            loggerMock.Object
        );
    }

    [Fact]
    public async Task CreateVenueAsync_ShouldAddVenue_WhenDtoIsValid()
    {
        var adminId = Guid.CreateVersion7();
        var dto = new CreateVenueDto("Venue", "Address", 100, "Desc", null);
        var user = new User(adminId.ToString(), "Admin", "User", UserRole.Admin, "admin@test.com", "admin@test.com", "1234567890", "+1");

        _createValidatorMock.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        
        _userRepoMock.Setup(r => r.GetByIdentityIdAsync(adminId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        await _service.CreateVenueAsync(adminId, dto);

        _venueRepoMock.Verify(r => r.AddAsync(It.Is<Venue>(v => v.Name == "Venue"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteVenueAsync_ShouldThrowInvalidOperation_WhenVenueHasEvents()
    {
        var venueId = Guid.CreateVersion7();
        _eventRepoMock.Setup(r => r.HasEventsAtVenueAsync(venueId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var userId = Guid.CreateVersion7();
        _userRepoMock.Setup(r => r.GetByIdentityIdAsync(userId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User("id", "F", "L", UserRole.Admin, "u", "e", null, "+1"));

        var venue = new Venue("Name", "Addr", 1, Guid.NewGuid());
        _venueRepoMock.Setup(r => r.GetByIdAsync(venueId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(venue);

        Func<Task> act = async () => await _service.DeleteVenueAsync(userId, venueId);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*associated with existing events*");
        _venueRepoMock.Verify(r => r.DeleteAsync(It.IsAny<Venue>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
