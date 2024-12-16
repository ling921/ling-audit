# Ling.Audit [![NuGet](https://img.shields.io/nuget/v/Ling.Audit.svg)](https://www.nuget.org/packages/Ling.Audit/)

A source generator for automatically generating audit properties in C# classes.

## Features

- Automatically generates audit-related properties based on implemented interfaces
- Supports the following audit properties:
  - Creation info (`CreatedBy`, `CreatedAt`)
  - Modification info (`ModifiedBy`, `ModifiedAt`)
  - Deletion info (`IsDeleted`, `DeletedBy`, `DeletedAt`)
- Handles nullable reference types properly
- Zero runtime dependencies

## Installation

1. Use .NET CLI:
```
dotnet add package Ling.Audit
```

2. Use NuGet Package Manager:

```
Install-Package Ling.Audit
```

## Usage

See [README.md](src/Ling.Audit/README.md)

## License

[Apache 2.0](LICENSE)
