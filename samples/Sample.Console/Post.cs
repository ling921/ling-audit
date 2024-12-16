using Ling.Audit;

namespace Sample.Console;

public partial class Post :
    IHasCreator<int?>,
    IHasCreationTime,
    IHasModifier<int?>,
    IHasModificationTime,
    ISoftDelete,
    IHasDeleter<int>,
    IHasDeletionTime
{
}

public partial class Base : IHasCreator<int?>
{

}