using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ling.Audit.EntityFrameworkCore.Tests.Core;

public abstract class AuditTestsBase : IDisposable
{
    private readonly SqliteConnection _connection;
    protected ServiceProvider ServiceProvider { get; }

    protected AuditTestsBase()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var services = new ServiceCollection();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Audit:AllowAnonymousCreate", "false" },
                { "Audit:AllowAnonymousModify", "false" },
                { "Audit:AllowAnonymousDelete", "false" },
                { "Audit:AuditNoFieldChangeEntity", "true" },
                { "Audit:Comments:CreatedBy", "Created By" },
                { "Audit:Comments:CreatedAt", "Created At" },
                { "Audit:Comments:ModifiedBy", "Modified By" },
                { "Audit:Comments:ModifiedAt", "Modified At" },
                { "Audit:Comments:DeletedBy", "Deleted By" },
                { "Audit:Comments:DeletedAt", "Deleted At" }
            })
            .Build();

        services.AddSingleton<IConfiguration>(configuration);

        services.AddDbContext<TestDbContext>(options =>
            ConfigureDbContext(options.UseInMemoryDatabase("TestDb")));

        ConfigureServices(services);

        ServiceProvider = services.BuildServiceProvider();
    }

    protected virtual DbContextOptionsBuilder ConfigureDbContext(DbContextOptionsBuilder options)
    {
        options.UseAudit<TestAuditContextProvider, int?>();

        return options;
    }

    protected virtual void ConfigureServices(IServiceCollection services)
    {
        // Override this method to configure additional services if needed
    }

    void IDisposable.Dispose()
    {
        _connection.Close();
        _connection.Dispose();
        ServiceProvider.Dispose();

        GC.SuppressFinalize(this);
    }
}
