# Ling.Audit [![NuGet](https://img.shields.io/nuget/v/Ling.Audit.svg)](https://www.nuget.org/packages/Ling.Audit/)

A source generator for automatically implementing audit properties in C# classes.

## Features

- 🚀 Zero runtime overhead - all code is generated at compile time
- 💡 Analyzer and code fixes for better audit interface usage
- 🔄 Supports inheritance and nested types
- 🛡️ Full type safety with generic support
- 📝 XML documentation included

## Quick Start

```shell
dotnet add package Ling.Audit
```

```csharp
// Just implement the interface
public partial class Post : IFullAudited<int?>
{
    public string Title { get; set; }
    public string Content { get; set; }
}

// All audit properties are automatically generated
// - CreatedBy, CreatedAt
// - LastModifiedBy, LastModifiedAt
// - IsDeleted, DeletedBy, DeletedAt
```

## Documentation

See [detailed documentation](src/Ling.Audit/README.md) for:
- Available interfaces and properties
- Advanced usage examples
- Best practices
- API reference

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

[Apache 2.0](LICENSE)
