using Ling.Audit.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Options;

namespace Ling.Audit.AspNetCore;

internal sealed class HttpAuditUserProvider(ICurrentDbContext current)
    : AuditContextProviderBase<string>(current)
{
    protected override AuditContext? GetAuditContext()
    {
        var httpContext = GetService<IHttpContextAccessor>()?.HttpContext;
        if (httpContext is null)
        {
            return null;
        }

        var principal = GetService<IAuditPrincipalResolver>()!.Resolve(httpContext);
        var options = GetService<IOptions<AspNetCoreAuditOptions>>()!.Value;
        var clientName = GetClientName(httpContext, options);

        return new AuditContext(
            principal.UserId,
            principal.UserName,
            httpContext.Connection.RemoteIpAddress?.ToString(),
            clientName);
    }

    private static string? GetClientName(HttpContext context, AspNetCoreAuditOptions options)
    {
        var userAgent = context.Request.Headers.UserAgent.ToString();
        if (string.IsNullOrWhiteSpace(userAgent) || options.MaxClientNameLength <= 0)
        {
            return null;
        }

        var value = options.ParseUserAgent
            ? TryParseUserAgent(userAgent) ?? userAgent
            : userAgent;

        return value.Length <= options.MaxClientNameLength
            ? value
            : value[..options.MaxClientNameLength];
    }

    private static string? TryParseUserAgent(string userAgent)
    {
        // UAParser is an optional integration. If the application references it, use it;
        // otherwise retain the original header without adding a mandatory package dependency.
        var parserType = Type.GetType("UAParser.Parser, UAParser", throwOnError: false);
        var getDefault = parserType?.GetMethod("GetDefault", Type.EmptyTypes);
        var parser = getDefault?.Invoke(null, null);
        var parse = parserType?.GetMethod("Parse", [typeof(string)]);
        return parse?.Invoke(parser, [userAgent])?.ToString();
    }
}
