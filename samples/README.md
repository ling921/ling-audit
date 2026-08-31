# Ling.Audit sample

This sample is a small ASP.NET Core Minimal API backed by SQLite. It demonstrates the three package layers working together:

- `Ling.Audit.Sample.Contracts` references only `Ling.Audit` and implements `IFullAudited<string>` on the `Order` entity;
- `Ling.Audit.Sample` references the EF Core and ASP.NET Core integrations;
- `UseAspNetCoreAudit` resolves the deterministic sample ClaimsPrincipal and writes the user, client, and timestamp fields;
- `RequireExplicitTransaction` makes the business save and audit-log save participate in the same transaction;
- deleting an order marks it as soft-deleted, so it disappears from the normal order query;
- `/audit` exposes the generated entity change logs for inspection.

Run it from the repository root:

```shell
dotnet run --project samples/Ling.Audit.Sample/Ling.Audit.Sample.csproj
```

Create an order and inspect its generated audit fields:

```shell
curl -X POST http://localhost:5000/orders \
  -H "Content-Type: application/json" \
  -d '{"description":"First sample order"}'

curl http://localhost:5000/orders
curl http://localhost:5000/audit
```

The sample uses `EnsureCreated` for simplicity. Production applications should create normal EF Core migrations after enabling auditing.
