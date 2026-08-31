namespace Ling.Audit.AspNetCore;

/// <summary>
/// Configures how audit user information is resolved from the current HTTP request.
/// </summary>
public sealed class AspNetCoreAuditOptions
{
    /// <summary>
    /// Gets or sets an explicit claim type for the user identifier. A null value enables automatic resolution.
    /// </summary>
    public string? UserIdClaimType { get; set; }

    /// <summary>
    /// Gets or sets an explicit claim type for the user name. A null value enables automatic resolution.
    /// </summary>
    public string? UserNameClaimType { get; set; }

    /// <summary>
    /// Gets or sets whether only authenticated identities may provide audit user information.
    /// </summary>
    public bool RequireAuthenticatedIdentity { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the User-Agent header should be parsed into a friendly client name.
    /// </summary>
    public bool ParseUserAgent { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum stored length of the client name or User-Agent value.
    /// </summary>
    public int MaxClientNameLength { get; set; } = 512;
}
