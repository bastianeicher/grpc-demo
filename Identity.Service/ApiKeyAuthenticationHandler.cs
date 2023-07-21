using System.Security.Principal;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Options;

namespace Identity.Service;

public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public ApiKeyAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        => Task.FromResult(Context.Request.Headers["x-api-key"].FirstOrDefault() switch
        {
            null or {Length: 0} => AuthenticateResult.NoResult(),
            "my-secret-key" => AuthenticateResult.Success(new AuthenticationTicket(new GenericPrincipal(new GenericIdentity("API key"), null), Scheme.Name)),
            _ => AuthenticateResult.Fail("Unknown API key")
        });
}