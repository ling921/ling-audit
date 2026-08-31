namespace Ling.Audit.EntityFrameworkCore.Tests.Core;

[AuditInclude(EntityOperationType.Create)]
internal sealed partial class CategoryEntity : ICreationAudited<int?>
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    [AuditIgnore]
    public int Sort { get; set; }

    public ICollection<PostEntity> Posts { get; set; } = new List<PostEntity>();
}
