# Ling.Audit

[English](README.md) | 简体中文

`Ling.Audit` 在编译期为领域实体生成强类型审计属性，通过 Entity Framework Core 自动维护这些属性，并可从 ASP.NET Core 获取当前用户和请求信息。核心契约不依赖 EF Core 或 ASP.NET Core，因此应用可以只引入实际需要的层。

## NuGet 包

| 包 | 用途 | 目标框架 |
| --- | --- | --- |
| [`Ling.Audit`](https://www.nuget.org/packages/Ling.Audit/) [![NuGet](https://img.shields.io/nuget/v/Ling.Audit.svg)](https://www.nuget.org/packages/Ling.Audit/) | 审计契约、源代码生成器、Analyzer 和 Code Fix。 | .NET Standard 2.0 |
| [`Ling.Audit.EntityFrameworkCore`](https://www.nuget.org/packages/Ling.Audit.EntityFrameworkCore/) [![NuGet](https://img.shields.io/nuget/v/Ling.Audit.EntityFrameworkCore.svg)](https://www.nuget.org/packages/Ling.Audit.EntityFrameworkCore/) | 审计属性维护、软删除以及实体/字段变更日志。 | .NET 6–10 |
| [`Ling.Audit.AspNetCore`](https://www.nuget.org/packages/Ling.Audit.AspNetCore/) [![NuGet](https://img.shields.io/nuget/v/Ling.Audit.AspNetCore.svg)](https://www.nuget.org/packages/Ling.Audit.AspNetCore/) | 从当前 HTTP 请求获取用户、IP 地址和客户端信息。 | .NET 6–10 |

EF Core 与 ASP.NET Core 包会为每个目标框架引用对应主版本的 EF Core。

## 功能

- 在编译期生成审计属性，无需反射或运行时代理类型。
- 提供创建、修改、删除和完整审计等粒度明确的接口。
- 支持 class、record、泛型类型、继承和嵌套 partial 类型。
- 通过编译器诊断与 Code Fix 提示无效的审计声明。
- 在 `SaveChanges` 时自动维护审计字段。
- 将被审计实体的删除转换为软删除，并与已有 Query Filter 合并。
- 记录实体级和字段级变更，支持复合主键和影子主键。
- 支持从 ASP.NET Core Identity、JWT Bearer、OpenID Connect 和自定义认证方案解析 Claims。
- 可替换用户提供器、Principal 解析器、序列化器、匿名处理器和时间提供器。
- 在业务写入与审计日志必须原子提交时提供显式事务模式。

## 快速开始

在定义实体的项目中安装核心包：

```shell
dotnet add package Ling.Audit --version 2.1.0
```

根据实体生命周期实现相应审计接口。实体类型及其所有包含类型都必须是 `partial`：

```csharp
using Ling.Audit;

namespace MyApplication.Domain;

public sealed partial class Post : IFullAudited<string>
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
}
```

源代码生成器会在当前编译中补充接口属性：

```csharp
public sealed partial class Post
{
    public string? CreatedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string? LastModifiedBy { get; set; }
    public DateTimeOffset? LastModifiedAt { get; set; }
    public bool IsDeleted { get; set; }
    public string? DeletedBy { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
```

只需要单个属性时使用基础接口，需要完整生命周期时使用组合接口：

| 接口 | 生成的属性 |
| --- | --- |
| `IHasCreator<TKey>` | `CreatedBy` |
| `IHasCreationTime` | `CreatedAt` |
| `IHasModifier<TKey>` | `LastModifiedBy` |
| `IHasModificationTime` | `LastModifiedAt` |
| `ISoftDelete` | `IsDeleted` |
| `IHasDeleter<TKey>` | `DeletedBy` |
| `IHasDeletionTime` | `DeletedAt` |
| `ICreationAudited<TKey>` | `CreatedBy`、`CreatedAt` |
| `IModificationAudited<TKey>` | `LastModifiedBy`、`LastModifiedAt` |
| `IDeletionAudited<TKey>` | `IsDeleted`、`DeletedBy`、`DeletedAt` |
| `IFullAudited<TKey>` | 全部创建、修改和删除属性 |

## Entity Framework Core

安装与应用目标框架相匹配的集成包：

```shell
dotnet add package Ling.Audit.EntityFrameworkCore --version 2.1.0
```

实现当前用户与客户端上下文提供器：

```csharp
using Ling.Audit.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

public sealed class ApplicationAuditUserProvider(
    ICurrentDbContext currentDbContext)
    : AuditContextProviderBase<Guid>(currentDbContext)
{
    protected override AuditContext? GetAuditContext()
    {
        var currentUser = GetService<ICurrentUser>();

        return currentUser?.Id is { } id
            ? new AuditContext(id, currentUser.Name, null, null)
            : null;
    }
}
```

在配置 `DbContext` 时启用审计：

```csharp
services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString);
    options.UseAudit<ApplicationAuditUserProvider, Guid>();
});
```

只有明确标记的实体会写入审计日志。实体属性默认参与审计，可以单独排除敏感或无意义的字段：

```csharp
using Ling.Audit.EntityFrameworkCore;

[Auditable]
public sealed partial class Account : IFullAudited<Guid>
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;

    [NotAudited]
    public string PasswordHash { get; set; } = string.Empty;
}
```

也可以使用 Fluent API 完成相同配置：

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<Account>().IsAuditable();
    modelBuilder.Entity<Account>()
        .Property(account => account.PasswordHash)
        .IsNotAudited();

    modelBuilder.SetupSoftDeleteQueryFilter();
    modelBuilder.ConfigureAuditLogTableNames(
        "AuditEntityChanges",
        "AuditFieldChanges");
}
```

`SetupSoftDeleteQueryFilter` 会把软删除条件与已有过滤器组合，不会覆盖多租户或权限过滤器。启用审计后应正常创建 EF Core Migration，将审计属性和日志表加入数据库。

### 事务保证

默认的 `BestEffort` 模式保留旧版本的两次保存行为。业务写入成功后，后续的审计日志保存仍可能失败。

需要原子性时，让应用显式持有事务：

```csharp
options.UseAudit<ApplicationAuditUserProvider, Guid>(audit =>
{
    audit.TransactionMode = AuditTransactionMode.RequireExplicitTransaction;
});
```

```csharp
await using var transaction =
    await dbContext.Database.BeginTransactionAsync(cancellationToken);

dbContext.Add(order);
await dbContext.SaveChangesAsync(cancellationToken);
await transaction.CommitAsync(cancellationToken);
```

此模式会拒绝没有活动关系型事务的审计写入，业务数据和生成的审计日志将参与同一个事务。

### 匿名操作

默认情况下，无法解析用户的审计操作会被拒绝。确实允许匿名访问时，可以按实体开放指定操作：

```csharp
[Auditable(AllowedAnonymous = DataOperation.Create)]
public sealed partial class Registration : ICreationAudited<string>
{
}
```

也可以通过 `AuditOptions.AllowAnonymous` 修改全局行为，或使用 `UseAnonymousHandler<THandler>()` 自定义处理方式。

## ASP.NET Core

如果应用使用字符串 Claim 作为用户 ID，可以直接安装 ASP.NET Core 包，无需自行实现 EF 用户提供器：

```shell
dotnet add package Ling.Audit.AspNetCore --version 2.1.0
```

```csharp
using Ling.Audit.AspNetCore;

services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString);
    options.UseAspNetCoreAudit();
});
```

默认解析器选择第一个已认证的 `ClaimsIdentity`，并按以下顺序取值：

1. 显式配置的 `UserIdClaimType` 或 `UserNameClaimType`。
2. 注册 ASP.NET Core Identity 时的 `IdentityOptions.ClaimsIdentity` ClaimType。
3. 使用 `ClaimsIdentity.Name` 获取用户名。
4. 用户 ID 依次回退到 `ClaimTypes.NameIdentifier` 和 `sub`。
5. 用户名依次回退到 `ClaimTypes.Name` 和 `name`。

可以在不修改应用认证设置的情况下覆盖 ClaimType：

```csharp
options.UseAspNetCoreAudit(http =>
{
    http.UserIdClaimType = "employee_id";
    http.UserNameClaimType = "preferred_username";
    http.MaxClientNameLength = 256;
});
```

没有 `HttpContext` 会被视为后台任务或非 HTTP 操作，并进入已配置的匿名策略。集成只读取 `HttpContext.User`，不会再次发起认证。

多 Identity 或应用具有特殊选择规则时，可以实现 `IAuditPrincipalResolver`：

```csharp
public sealed class ApplicationPrincipalResolver : IAuditPrincipalResolver
{
    public AuditPrincipal Resolve(HttpContext httpContext)
    {
        var user = httpContext.User;
        return new AuditPrincipal(
            user.FindFirst("actor_id")?.Value,
            user.Identity?.Name);
    }
}
```

```csharp
options.UseAspNetCoreAudit<ApplicationPrincipalResolver>();
```

`Ling.Audit.AspNetCore` 明确使用字符串用户 ID。使用 `Guid`、`int` 或其他键类型的应用应使用 `Ling.Audit.EntityFrameworkCore`，并注册相应的 `AuditContextProviderBase<TUserId>` 实现。

## 自定义扩展

- `UseSerializer<TSerializer>()` 替换属性序列化器。
- `UseAnonymousHandler<THandler>()` 处理被拒绝的匿名操作。
- `UseAuditTimeProvider<TTimeProvider>()` 提供可测试的 UTC 时间。
- `UseAspNetCoreAudit<TPrincipalResolver>()` 修改 HTTP Principal 选择规则。

## 设计

- `Ling.Audit` 负责面向领域层的契约和编译期工具，不依赖 EF Core 或 ASP.NET Core。
- `Ling.Audit.EntityFrameworkCore` 负责持久化行为、软删除和变更日志模型，不依赖 HTTP 或 Claims API。
- `Ling.Audit.AspNetCore` 将当前请求适配到 EF Core 用户提供器契约；Principal 解析可以替换，并且不会主动执行认证。

这种边界使实体契约可以复用于 Web 应用、Worker、测试和其他宿主，同时让请求相关行为保持可选。

## 从 1.x 迁移

原来的 `Ling.EntityFrameworkCore.Audit` 已更名为 `Ling.Audit.EntityFrameworkCore`。ASP.NET Core Claim 设置移动到了独立的 ASP.NET Core 集成中，匿名审计操作现在默认拒绝。

包名、API、事务和数据格式变化请参阅 [2.0 迁移指南](docs/migrating-to-2.0.md)。

## 开发

```shell
dotnet build Ling.Audit.sln -c Release
dotnet test Ling.Audit.sln -c Release
```

测试矩阵会在 .NET 6、7、8、9 和 10 上分别运行 EF Core 与 ASP.NET Core 集成测试。行为变更应同时包含相应测试。

## 协议

[Apache License 2.0](LICENSE)
