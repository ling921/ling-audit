using Ling.Audit.EntityFrameworkCore.Tests.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ling.Audit.EntityFrameworkCore.Tests;

public class TestClass : AuditTestsBase
{
    public TestClass()
    {
        using var scope = ServiceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        context.Database.EnsureCreated();

        var category = new CategoryEntity
        {
            Name = "Test Category",
            Sort = 1,
        };

        context.Posts.AddRange(new[]
        {
            new PostEntity
            {
                Title = "Test Post 1",
                Content = "Content 1",
                Category = category,
            },
            new PostEntity
            {
                Title = "Test Post 2",
                Content = "Content 2",
                Category = category,
            },
        });

        context.SaveChanges();
    }

    [Fact]
    public async Task RunAsync()
    {
        using var scope = ServiceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TestDbContext>();

        var categories = await context.Categories
            .Include(e => e.Posts)
            .ToListAsync();

        categories
            .Should().ContainSingle().Which
            .Should().BeEquivalentTo(new
            {
                Name = "Test Category",
                Sort = 1,
                CreatedBy = 1,
                Posts = new object[]
                {
                    new
                    {
                        Title = "Test Post 1",
                        Content = "Content 1",
                        CreatedBy = 1,
                    },
                    new
                    {
                        Title = "Test Post 2",
                        Content = "Content 2",
                        CreatedBy = 1,
                    }
                }
            });

        var auditLogs = await context.Set<AuditEntityChangeLog<int?>>()
            .Include(e => e.Details)
            .ToListAsync();

        const string? @null = null;
        auditLogs.Should().BeEquivalentTo(new object[]
        {
            new
            {
                DatabaseSchema = @null,
                TableName = "Categories",
                EntityKey = $"{nameof(CategoryEntity.Id)}={1}",
                EntityTypeName = nameof(CategoryEntity),
                EventType = AuditEventType.Create,
                UserId = 1,
                UserName = "Tom",
                IPAddress = "192.168.1.10",
                ClientName = "test client",
                Details = new object[]
                {
                    new
                    {
                        FieldName = nameof(CategoryEntity) + "." + nameof(CategoryEntity.Id),
                        OriginalValue = @null,
                        NewValue = "1",
                    },
                    new
                    {
                        FieldName = nameof(CategoryEntity) + "." + nameof(CategoryEntity.Name),
                        OriginalValue = (string?)null,
                        NewValue = "Test Category",
                    },
                },
            },
            new
            {
                DatabaseSchema = @null,
                TableName = "Posts",
                EntityKey = $"{nameof(PostEntity.Id)}={1}",
                EntityTypeName = nameof(PostEntity),
                EventType = AuditEventType.Create,
                UserId = 1,
                UserName = "Tom",
                IPAddress = "192.168.1.10",
                ClientName = "test client",
                Details = new List<AuditFieldChangeLog>
                {
                    new()
                    {
                        FieldName = nameof(PostEntity) + "." + nameof(PostEntity.Id),
                        OriginalValue = @null,
                        NewValue = "1",
                    },
                    new()
                    {
                        FieldName = nameof(PostEntity) + "." + nameof(PostEntity.Title),
                        OriginalValue = null,
                        NewValue = "Test Post 1",
                    },
                    new()
                    {
                        FieldName = nameof(PostEntity) + "." + nameof(PostEntity.Content),
                        OriginalValue = @null,
                        NewValue = "Content 1",
                    },
                },
            },
            new
            {
                DatabaseSchema = @null,
                TableName = "Posts",
                EntityKey = $"{nameof(PostEntity.Id)}={2}",
                EntityTypeName = nameof(PostEntity),
                EventType = AuditEventType.Create,
                UserId = 1,
                UserName = "Tom",
                IPAddress = "192.168.1.10",
                ClientName = "test client",
                Details = new List<AuditFieldChangeLog>
                {
                    new()
                    {
                        FieldName = nameof(PostEntity) + "." + nameof(PostEntity.Id),
                        OriginalValue = @null,
                        NewValue = "2",
                    },
                    new()
                    {
                        FieldName = nameof(PostEntity) + "." + nameof(PostEntity.Title),
                        OriginalValue = @null,
                        NewValue = "Test Post 2",
                    },
                    new()
                    {
                        FieldName = nameof(PostEntity) + "." + nameof(PostEntity.Content),
                        OriginalValue = @null,
                        NewValue = "Content 2",
                    },
                },
            }
        }, options => options.WithoutStrictOrdering());
    }
}
