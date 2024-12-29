using Ling.Audit.EntityFrameworkCore;
using Ling.Audit.EntityFrameworkCore.Internal.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using UAParser;

namespace Ling.Audit.AspNetCore;

/// <summary>
/// Default implementation of interface <see cref="IAuditContextProvider{T}"/>.
/// </summary>
internal sealed class HttpAuditContextProvider : IAuditContextProvider<string?>
{
    private readonly Lazy<AuditContext> _lazyContext;

    /// <inheritdoc/>
    public string? Id => _lazyContext.Value.Id;

    /// <inheritdoc/>
    public string? Name => _lazyContext.Value.Name;

    /// <inheritdoc/>
    public string? IPAddress => _lazyContext.Value.IPAddress;

    /// <inheritdoc/>
    public string? ClientName => _lazyContext.Value.ClientName;

    public HttpAuditContextProvider(ICurrentDbContext current)
    {
        _lazyContext = new Lazy<AuditContext>(() => new AuditContext(
            current.Context.GetService<ILogger<HttpAuditContextProvider>>(),
            current.Context.GetService<IHttpContextAccessor>(),
            current.Context.GetAuditOptions()));
    }

    private class AuditContext : IAuditContextProvider<string?>
    {
        public string? Id { get; }
        public string? Name { get; }
        public string? IPAddress { get; }
        public string? ClientName { get; }

        public AuditContext(ILogger logger, IHttpContextAccessor httpContextAccessor, AuditOptions options)
        {
            var context = httpContextAccessor.HttpContext;
            if (context is null)
            {
                logger.LogWarning("HttpContext is null for the current request.");
                return;
            }

            Id = context.User.FindFirstValue(options.UserIdClaimType);
            Name = context.User.FindFirstValue(options.UserNameClaimType);
            IPAddress = context.Connection.RemoteIpAddress?.ToString();

            var userAgent = context.Request.Headers.UserAgent;
            if (!string.IsNullOrEmpty(userAgent))
            {
                ClientName = Parser.GetDefault().Parse(userAgent).ToString();
            }
            else
            {
                logger.LogWarning("User-Agent header is missing.");
            }
        }
    }
}
