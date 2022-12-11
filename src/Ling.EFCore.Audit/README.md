### What is this library?

Ling.EntityFrameworkCore.Audit is an extension library that can automatically record entity changes of `Microsoft.EntityFrameworkCore`.

### How do I get started?

```csharp
builder.Services.Addxxx<xxDbContext>(
    connectionString,
    optionsAction: options => options.UseAudit());
```
