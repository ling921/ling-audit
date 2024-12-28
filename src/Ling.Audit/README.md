# Ling.Audit

A source generator for automatically implementing audit properties in C# classes.

## Basic Usage

```csharp
// Implement the interface you need
public partial class Post : ICreationAudited<int?>
{
    public string Title { get; set; }
}

// Properties are automatically generated
partial class Post
{
    public int? CreatedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
```

## Available Interfaces

### Basic Interfaces

| Interface | Properties | Description |
|-----------|------------|-------------|
| `IHasCreator<TKey>` | `CreatedBy` | Records who created the entity |
| `IHasCreationTime` | `CreatedAt` | Records when the entity was created |
| `IHasModifier<TKey>` | `LastModifiedBy` | Records who last modified the entity |
| `IHasModificationTime` | `LastModifiedAt` | Records when the entity was last modified |
| `ISoftDelete` | `IsDeleted` | Marks if the entity is soft deleted |
| `IHasDeleter<TKey>` | `DeletedBy` | Records who deleted the entity |
| `IHasDeletionTime` | `DeletedAt` | Records when the entity was deleted |

### Composite Interfaces

| Interface | Properties | Description |
|-----------|------------|-------------|
| `ICreationAudited<TKey>` | `CreatedBy`, `CreatedAt` | Full creation auditing |
| `IModificationAudited<TKey>` | `LastModifiedBy`, `LastModifiedAt` | Full modification auditing |
| `IDeletionAudited<TKey>` | `IsDeleted`, `DeletedBy`, `DeletedAt` | Full deletion auditing |
| `IFullAudited<TKey>` | All above | Complete auditing support |

## Advanced Usage

### Using Multiple Basic Interfaces

```csharp
public partial class Post :
    IHasCreator<int?>,      // Adds CreatedBy
    IHasCreationTime        // Adds CreatedAt
{
}
```

> 💡 **Tip**: Use `ICreationAudited<TKey>` instead for better maintainability.

### Sealed Classes

```csharp
public sealed partial class Post : ICreationAudited<int?>
{
    // Generated properties won't be virtual
}
```

### Generic Types

```csharp
public partial class Post<TKey> : ICreationAudited<TKey>
    where TKey : struct
{
}
```

### Nested Types

```csharp
public partial class Blog
{
    public partial class Post : ICreationAudited<int?>
    {
    }
}
```

## Best Practices

1. Use composite interfaces instead of basic ones
2. Make your classes `partial`
3. Use consistent key types across your entities
4. Consider using nullable reference types
5. Follow the analyzer suggestions

## API Reference

For detailed API documentation, visit our [GitHub repository](https://github.com/ling921/ling-audit).
