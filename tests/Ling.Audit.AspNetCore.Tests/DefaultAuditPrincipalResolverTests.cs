using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Ling.Audit.AspNetCore.Tests;

public sealed class DefaultAuditPrincipalResolverTests
{
    [Fact]
    public void Resolve_ExplicitClaimTypesTakePrecedence()
    {
        var identityOptions = new IdentityOptions();
        identityOptions.ClaimsIdentity.UserIdClaimType = "identity-id";
        var resolver = CreateResolver(new AspNetCoreAuditOptions
        {
            UserIdClaimType = "audit-id",
            UserNameClaimType = "audit-name",
        }, identityOptions);
        var context = CreateContext(new ClaimsIdentity(
        [
            new Claim("audit-id", "explicit-id"),
            new Claim("identity-id", "identity-id"),
            new Claim("audit-name", "Explicit Name"),
        ], "test"));

        var principal = resolver.Resolve(context);

        principal.UserId.Should().Be("explicit-id");
        principal.UserName.Should().Be("Explicit Name");
    }

    [Fact]
    public void Resolve_UsesIdentityNameAndIdentityOptionsUserId()
    {
        var identityOptions = new IdentityOptions();
        identityOptions.ClaimsIdentity.UserIdClaimType = "custom-id";
        var resolver = CreateResolver(new AspNetCoreAuditOptions(), identityOptions);
        var identity = new ClaimsIdentity(
            [new Claim("custom-id", "42"), new Claim("custom-name", "Alice")],
            "test",
            "custom-name",
            ClaimTypes.Role);

        var principal = resolver.Resolve(CreateContext(identity));

        principal.UserId.Should().Be("42");
        principal.UserName.Should().Be("Alice");
    }

    [Fact]
    public void Resolve_FallsBackToSubjectClaim()
    {
        var resolver = CreateResolver(new AspNetCoreAuditOptions(), new IdentityOptions());
        var identity = new ClaimsIdentity([new Claim("sub", "subject-id")], "jwt");

        resolver.Resolve(CreateContext(identity)).UserId.Should().Be("subject-id");
    }

    [Fact]
    public void Resolve_SelectsFirstAuthenticatedIdentity()
    {
        var resolver = CreateResolver(new AspNetCoreAuditOptions(), new IdentityOptions());
        var anonymous = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "anonymous")]);
        var authenticated = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "authenticated")], "cookie");
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal([anonymous, authenticated]),
        };

        resolver.Resolve(context).UserId.Should().Be("authenticated");
    }

    [Fact]
    public void Resolve_ReturnsAnonymousWhenNoAuthenticatedIdentityExists()
    {
        var resolver = CreateResolver(new AspNetCoreAuditOptions(), new IdentityOptions());
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "ignored")]);

        resolver.Resolve(CreateContext(identity)).Should().Be(new AuditPrincipal(null, null));
    }

    private static DefaultAuditPrincipalResolver CreateResolver(
        AspNetCoreAuditOptions options,
        IdentityOptions identityOptions) =>
        new(Options.Create(options), Options.Create(identityOptions));

    private static DefaultHttpContext CreateContext(ClaimsIdentity identity) => new()
    {
        User = new ClaimsPrincipal(identity),
    };
}
