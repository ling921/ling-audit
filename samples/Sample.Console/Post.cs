using Ling.Audit;

namespace Sample.Console;

public partial class Post :
    IHasCreator<int?>,
    IHasCreationTime,
    IHasLastModifier<int?>,
    IHasLastModificationTime,
    ISoftDelete,
    IHasDeleter<int?>,
    IHasDeletionTime
{
}
