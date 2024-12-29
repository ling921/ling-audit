namespace Ling.Audit.EntityFrameworkCore;

/// <summary>
/// Interface for audit user information.
/// </summary>
public interface IAuditContextProvider<TUserId>
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
