using Ling.Audit;

namespace Sample.Console;

public partial class Post :
    EntityBase,
    ISoftDelete,
    IHasCreator<int?>,
    IHasDeleter<int?>,
    IHasCreationTime,
    IHasDeletionTime
{
    public virtual int? CreatedBy { get; set; }
}

public partial class Post :
    IHasModifier<int?>,
    IHasModificationTime,
    ISoftDelete,
    IHasDeleter<int?>,
    IHasDeletionTime{
    public virtual int? LastModifiedBy { get; set; }
}

public class EntityBase { }
