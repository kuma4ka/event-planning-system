using FluentAssertions;

namespace EventPlanning.IntegrationTests;

public class BasicTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory = factory;
    private readonly HttpClient _client = factory.CreateClient();

    [Theory]
    [InlineData("/")]
    [InlineData("/Home/Index")]
    [InlineData("/Account/Login")]
    [InlineData("/Account/Register")]
    public async Task Get_EndpointsReturnSuccessAndCorrectContentType(string url)
    {
        var response = await _client.GetAsync(url);

        response.EnsureSuccessStatusCode(); 
        response.Content.Headers.ContentType!.ToString().Should().Contain("text/html");
    }
}
