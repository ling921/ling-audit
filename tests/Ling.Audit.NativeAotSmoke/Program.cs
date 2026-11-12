using Ling.Audit;

var entity = new AuditedOrder
{
    CreatedBy = "aot-user",
    CreatedAt = DateTimeOffset.UtcNow,
    IsDeleted = false,
};

if (entity.CreatedBy != "aot-user" || entity.IsDeleted)
{
    return 1;
}

Console.WriteLine("Ling.Audit Native AOT smoke passed.");
return 0;

public sealed partial class AuditedOrder : IHasCreator<string>, IHasCreationTime, ISoftDelete
{
}
