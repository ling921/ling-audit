## Usage

1. Implement the desired audit interfaces on your class:

```csharp
public partial class Post :
    IHasCreator<int?>,      // Adds CreatedBy property
    IHasCreationTime,       // Adds CreatedAt property
    IHasModifier<int?>,     // Adds LastModifiedBy property
    IHasModificationTime,   // Adds LastModifiedAt property
    ISoftDelete,            // Adds IsDeleted property
    IHasDeleter<int?>,      // Adds DeletedBy and DeletedAt properties
    IHasDeletionTime        // Adds DeletedAt property
{
    public string Title { get; set; }
    public string Content { get; set; }
}
```

2. The source generator will automatically generate the audit properties:

```csharp
partial class Post
{
    public int? CreatedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public int? LastModifiedBy { get; set; }
    public DateTimeOffset? LastModifiedAt { get; set; }
    public bool IsDeleted { get; set; }
    public int? DeletedBy { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
```


## Supported Interfaces & Properties

| Interface | Properties |
|-----------|------------|
| `IHasCreator<TKey>` | `CreatedBy` |
| `IHasCreationTime` | `CreatedAt` |
| `IHasModifier<TKey>` | `LastModifiedBy` |
| `IHasModificationTime` | `LastModifiedAt` |
| `ISoftDelete` | `IsDeleted` |
| `IHasDeleter<TKey>` | `DeletedBy`, `DeletedAt` |
| `IHasDeletionTime` | `DeletedAt` |
| `ICreationAudited<TKey>` | `CreatedBy`, `CreatedAt` |
| `IModificationAudited<TKey>` | `LastModifiedBy`, `LastModifiedAt` |
| `IDeletionAudited<TKey>` | `IsDeleted`, `DeletedBy`, `DeletedAt` |
| `IFullAudited<TKey>` | `CreatedBy`, `CreatedAt`, `LastModifiedBy`, `LastModifiedAt`, `IsDeleted`, `DeletedBy`, `DeletedAt` |
