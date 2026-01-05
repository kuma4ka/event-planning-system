using EventPlanning.Application.Interfaces;
using EventPlanning.Application.Services;
using EventPlanning.Domain.Entities;
using EventPlanning.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;

namespace EventPlanning.Tests.Services;

public class NewsletterServiceTests
{
    private readonly Mock<INewsletterRepository> _repoMock;
    private readonly Mock<IEmailService> _emailMock;
    private readonly NewsletterService _service;

    public NewsletterServiceTests()
    {
        _repoMock = new Mock<INewsletterRepository>();
        _emailMock = new Mock<IEmailService>();
        Mock<IHttpContextAccessor> httpMock = new Mock<IHttpContextAccessor>();
        Mock<ILogger<NewsletterService>> loggerMock = new Mock<ILogger<NewsletterService>>();

        _service = new NewsletterService(
            _repoMock.Object,
            _emailMock.Object,
            httpMock.Object,
            loggerMock.Object
        );
    }

    [Fact]
    public async Task SubscribeAsync_ShouldSendEmail_WhenNewSubscriber()
    {
        var email = "test@example.com";
        _repoMock.Setup(r => r.GetSubscriberByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((NewsletterSubscriber?)null);

        await _service.SubscribeAsync(email);

        _repoMock.Verify(
            r => r.AddSubscriberAsync(It.Is<NewsletterSubscriber>(s => s.Email == email),
                It.IsAny<CancellationToken>()), Times.Once);
        _emailMock.Verify(
            e => e.SendEmailAsync(email, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ConfirmSubscriptionAsync_ShouldReturnFalse_WhenTokenMismatch()
    {
        var email = "test@example.com";
        var subscriber = new NewsletterSubscriber { Email = email, ConfirmationToken = "valid-token" };

        _repoMock.Setup(r => r.GetSubscriberByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscriber);

        var result = await _service.ConfirmSubscriptionAsync(email, "invalid-token");

        result.Should().BeFalse();
        subscriber.IsConfirmed.Should().BeFalse();
    }

    [Fact]
    public async Task ConfirmSubscriptionAsync_ShouldReturnTrue_WhenTokenMatches()
    {
        var email = "test@example.com";
        var subscriber = new NewsletterSubscriber
            { Email = email, ConfirmationToken = "valid-token", IsConfirmed = false };

        _repoMock.Setup(r => r.GetSubscriberByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscriber);

        var result = await _service.ConfirmSubscriptionAsync(email, "valid-token");

        result.Should().BeTrue();
        subscriber.IsConfirmed.Should().BeTrue();
        subscriber.ConfirmationToken.Should().BeNull();
        _repoMock.Verify(r => r.UpdateSubscriberAsync(subscriber, It.IsAny<CancellationToken>()), Times.Once);
    }
}