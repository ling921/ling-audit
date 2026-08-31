using Ling.Audit;

namespace Ling.Audit.Sample.Contracts;

public sealed partial class Order : IFullAudited<string>
{
    public int Id { get; set; }

    public string Description { get; set; } = string.Empty;
}
