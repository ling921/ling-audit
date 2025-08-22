using Ling.Audit.EntityFrameworkCore.Tests.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace Ling.Audit.EntityFrameworkCore.Tests.Base;

public abstract class TestBase : IDisposable
{
    private bool _disposedValue;
    private readonly IServiceScope _scope = default!;
    private readonly DbContextOptions<TestDbContext> _dbContextOptions = default!;

    protected TestBase()
    {
        var services = new ServiceCollection();

        var auditOptions = new AuditOptions();
        ConfigureAuditOptions(auditOptions);
        using var auditOptionsStream = new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(auditOptions));

        var configuration = new ConfigurationBuilder()
            .AddJsonStream(auditOptionsStream)
            .Build();
        services.AddSingleton<IConfiguration>(configuration);

        var builder = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase("TestDB");

        ConfigureDbContext(builder);

        _dbContextOptions = builder.Options;

        services.AddScoped(_ => new TestDbContext(_dbContextOptions));

        ConfigureServices(services);

        var serviceProvider = services.BuildServiceProvider();
        _scope = serviceProvider.CreateAsyncScope();
    }

    protected TService GetService<TService>() where TService : notnull
    {
        return _scope.ServiceProvider.GetRequiredService<TService>();
    }

    protected virtual void ConfigureAuditOptions(AuditOptions options)
    {
    }

    protected virtual void ConfigureDbContext(DbContextOptionsBuilder<TestDbContext> builder)
    {
        builder.UseAudit<TestAuditContextProvider, int?>();
    }

    protected virtual void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped(_ => new TestDbContext(_dbContextOptions));
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _scope?.Dispose();
            }

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
