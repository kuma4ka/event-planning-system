using FluentAssertions;

namespace EventPlanning.IntegrationTests;

public class BasicTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public BasicTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/Home/Index")]
    [InlineData("/Account/Login")]
    [InlineData("/Account/Register")]
    public async Task Get_EndpointsReturnSuccessAndCorrectContentType(string url)
    {
        // Act
        var response = await _client.GetAsync(url);

        // Assert
        response.EnsureSuccessStatusCode(); 
        response.Content.Headers.ContentType!.ToString().Should().Contain("text/html");
    }
}
