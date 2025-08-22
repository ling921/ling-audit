namespace Ling.Audit.EntityFrameworkCore.Tests.Core;

internal sealed partial class PostEntity : IFullAudited<int?>
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public CategoryEntity Category { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
}