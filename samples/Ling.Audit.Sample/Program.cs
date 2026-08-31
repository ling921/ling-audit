using System.Security.Claims;
using Ling.Audit.EntityFrameworkCore;
using Ling.Audit.Sample.Contracts;
using Ling.Audit.AspNetCore;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<SampleDbContext>(options =>
{
    options.UseSqlite("Data Source=ling-audit-sample.db");
    options.UseAspNetCoreAudit(
        audit => audit.MaxClientNameLength = 256,
        audit => audit.TransactionMode = AuditTransactionMode.RequireExplicitTransaction);
});

var app = builder.Build();

// A real application would install authentication middleware here. The sample uses
// a deterministic principal so the generated audit records are easy to inspect.
app.Use(async (context, next) =>
{
    var identity = new ClaimsIdentity(
    [
        new Claim(ClaimTypes.NameIdentifier, "sample-user"),
        new Claim(ClaimTypes.Name, "Sample User"),
    ],
    authenticationType: "Sample");
    context.User = new ClaimsPrincipal(identity);

    await next(context);
});

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SampleDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.MapGet("/orders", async (SampleDbContext db, CancellationToken cancellationToken) =>
    await db.Orders
        .Where(order => !order.IsDeleted)
        .Select(order => new
        {
            order.Id,
            order.Description,
            order.CreatedAt,
            order.CreatedBy,
            order.LastModifiedAt,
            order.LastModifiedBy,
        })
        .ToListAsync(cancellationToken));

app.MapPost("/orders", async (
    CreateOrderRequest request,
    SampleDbContext db,
    CancellationToken cancellationToken) =>
{
    await using var transaction =
        await db.Database.BeginTransactionAsync(cancellationToken);

    var order = new Order { Description = request.Description };
    db.Orders.Add(order);
    await db.SaveChangesAsync(cancellationToken);
    await transaction.CommitAsync(cancellationToken);

    return Results.Created($"/orders/{order.Id}", new
    {
        order.Id,
        order.Description,
        order.CreatedAt,
        order.CreatedBy,
    });
});

app.MapDelete("/orders/{id:int}", async (
    int id,
    SampleDbContext db,
    CancellationToken cancellationToken) =>
{
    var order = await db.Orders.FindAsync([id], cancellationToken);
    if (order is null)
    {
        return Results.NotFound();
    }

    await using var transaction =
        await db.Database.BeginTransactionAsync(cancellationToken);

    db.Orders.Remove(order);
    await db.SaveChangesAsync(cancellationToken);
    await transaction.CommitAsync(cancellationToken);

    return Results.NoContent();
});

app.MapGet("/audit", async (SampleDbContext db, CancellationToken cancellationToken) =>
    await db.EntityChanges
        .OrderByDescending(change => change.Id)
        .Select(change => new
        {
            change.Id,
            change.EntityTypeName,
            change.EntityKey,
            change.EventType,
            change.EventTime,
            change.UserId,
            change.UserName,
            change.ClientName,
        })
        .ToListAsync(cancellationToken));

app.Run();

public sealed record CreateOrderRequest(string Description);

public sealed class SampleDbContext(DbContextOptions<SampleDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();

    public DbSet<AuditEntityChangeLog<string>> EntityChanges =>
        Set<AuditEntityChangeLog<string>>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>().IsAuditable();
        modelBuilder.SetupSoftDeleteQueryFilter();
    }
}
