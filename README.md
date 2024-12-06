# Ling.Audit

A source generator for automatically generating audit properties in C# classes.

## Features

- Automatically generates audit-related properties based on implemented interfaces
- Supports the following audit properties:
  - Creation info (CreatedBy, CreatedAt)
  - Modification info (ModifiedBy, ModifiedAt)
  - Deletion info (DeletedBy, DeletedAt, IsDeleted)
- Handles nullable reference types properly
- Zero runtime dependencies

## Installation

```
dotnet add package Ling.Audit
```


## Usage

1. Implement the desired audit interfaces on your class:

```csharp
public partial class Post :
    ICreatedBy<int>, // Adds CreatedBy property
    ICreatedAt, // Adds CreatedAt property
    IModifiedBy<int>, // Adds ModifiedBy property
    IModifiedAt, // Adds ModifiedAt property
    ISoftDelete, // Adds IsDeleted property
    IHasDeleter<int>, // Adds DeletedBy and DeletedAt properties
    IHasDeletionTime // Adds DeletedAt property
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
    public int? ModifiedBy { get; set; }
    public DateTimeOffset? ModifiedAt { get; set; }
    public bool IsDeleted { get; set; }
    public int? DeletedBy { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
```


## Supported Interfaces

- `ICreatedBy<TKey>` - Adds `CreatedBy` property
- `ICreatedAt` - Adds `CreatedAt` property
- `IModifiedBy<TKey>` - Adds `ModifiedBy` property
- `IModifiedAt` - Adds `ModifiedAt` property
- `ISoftDelete` - Adds `IsDeleted` property
- `IHasDeleter<TKey>` - Adds `DeletedBy` and `DeletedAt` properties
- `IHasDeletionTime` - Adds `DeletedAt` property
- `ICreationAudited<TKey>` - Adds `CreatedBy` and `CreatedAt` properties
- `IModificationAudited<TKey>` - Adds `ModifiedBy` and `ModifiedAt` properties
- `IDeletionAudited<TKey>` - Adds `DeletedBy` and `DeletedAt` properties
- `IAudited<TKey>` - Adds `CreatedBy`, `CreatedAt`, `ModifiedBy`, `ModifiedAt`, `DeletedBy`, `DeletedAt`, and `IsDeleted` properties

## License

[Apache 2.0](LICENSE)
