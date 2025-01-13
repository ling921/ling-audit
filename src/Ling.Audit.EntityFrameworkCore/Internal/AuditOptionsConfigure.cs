using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Ling.Audit.EntityFrameworkCore.Internal;

internal class AuditOptionsConfigure(IConfiguration config)
    : ConfigureFromConfigurationOptions<AuditOptions>(config.GetSection(Constants.ConfigurationSection));
