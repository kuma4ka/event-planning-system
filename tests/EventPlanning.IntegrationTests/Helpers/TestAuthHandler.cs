using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EventPlanning.IntegrationTests.Helpers;

public class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string AuthenticationScheme = "Test";
    public const string UserIdHeader = "X-Test-UserId";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(UserIdHeader, out var userId))
        {
            return Task.FromResult(AuthenticateResult.Fail("No User ID Provided"));
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()!),
            new(ClaimTypes.Name, "Test User"),
            new(ClaimTypes.Email, "test@example.com") 
        };

        if (Request.Headers.TryGetValue("X-Test-Role", out var role))
        {
            claims.Add(new Claim(ClaimTypes.Role, role.ToString()));
        }

        var identity = new ClaimsIdentity(claims, AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, AuthenticationScheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
