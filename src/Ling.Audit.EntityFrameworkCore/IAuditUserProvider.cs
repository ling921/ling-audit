using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Ling.Audit.EntityFrameworkCore;

/// <summary>
/// Interface for audit user information.
/// </summary>
public interface IAuditUserProvider<[MustNull] TUserId>
{
    /// <summary>
    /// Gets the identity of user.
    /// </summary>
    TUserId? Id { get; }

    /// <summary>
    /// Gets the name of user.
    /// </summary>
    string? Name { get; }

    /// <summary>
    /// Gets the IP address of the user.
    /// </summary>
    string? IPAddress { get; }

    /// <summary>
    /// Gets the client name or device name of the user.
    /// </summary>
    string? ClientName { get; }
}

/// <summary>
/// Provides an abstract base for managing audit context, including user and client information.
/// </summary>
/// <typeparam name="TUserId">Used to represent the unique identifier for the user associated with the audit context.</typeparam>
public abstract class AuditContextProviderBase<TUserId> : IAuditUserProvider<TUserId>
{
    private readonly ICurrentDbContext _currentDbContext;
    private readonly Lazy<AuditContext?> _lazyContext;

    /// <inheritdoc/>
    public TUserId? Id => _lazyContext.Value is null ? default : _lazyContext.Value.Id;

    /// <inheritdoc/>
    public string? Name => _lazyContext.Value?.Name;

    /// <inheritdoc/>
    public string? IPAddress => _lazyContext.Value?.IPAddress;

    /// <inheritdoc/>
    public string? ClientName => _lazyContext.Value?.ClientName;

    /// <summary>
    /// Initializes an instance of the AuditContextProviderBase class with a database context provider.
    /// </summary>
    /// <param name="current">Provides access to the current database context for auditing purposes.</param>
    protected AuditContextProviderBase(ICurrentDbContext current)
    {
        _currentDbContext = current;
        _lazyContext = new Lazy<AuditContext?>(GetAuditContext);
    }

    /// <summary>
    /// Retrieves a service of a specified type from the current database context.
    /// </summary>
    /// <typeparam name="TService">Represents the type of service to be retrieved from the database context.</typeparam>
    /// <returns>Returns the requested service instance or null if not found.</returns>
    protected TService? GetService<TService>() where TService : class
    {
        return _currentDbContext.Context.GetService<TService>();
    }

    /// <summary>
    /// Retrieves the current audit context, which may be null. It is an abstract method that must be implemented in
    /// derived classes.
    /// </summary>
    /// <returns>An optional AuditContext object representing the current audit state.</returns>
    protected abstract AuditContext? GetAuditContext();

    /// <summary>
    /// Represents the context for an audit event, including user identification and client details.
    /// </summary>
    /// <param name="Id">Represents the unique identifier of the user associated with the audit event.</param>
    /// <param name="Name">Holds the name of the user involved in the audit event.</param>
    /// <param name="IPAddress">Contains the IP address from which the user accessed the system.</param>
    /// <param name="ClientName">Stores the name of the client application used by the user.</param>
    protected sealed record AuditContext(
        TUserId? Id,
        string? Name,
        string? IPAddress,
        string? ClientName) : IAuditUserProvider<TUserId>;
}
