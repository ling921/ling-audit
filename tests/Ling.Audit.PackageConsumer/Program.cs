using Ling.Audit;

var entity = new ConsumerEntity();
Console.WriteLine(entity.CreatedAt);

public partial class ConsumerEntity : ICreationAudited<string>
{
}
