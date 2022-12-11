### What is this library?

Ling.Audit is a source generator for audit properties.

### Introduction

Ling.Audit is a source generator for audit properties.

### Installation

First, [install NuGet](http://docs.nuget.org/docs/start-here/installing-nuget). Then, install [Ling.Audit](https://www.nuget.org/packages/Ling.Audit/) from the package manager console:

```
PM> Install-Package Ling.Audit
```
Or from the .NET CLI as:
```
dotnet add package Ling.Audit
```

### Usage

Create your class with `partial` keyword and interface
`IHasCreationTime`, `IHasCreator<T>`, `ICreationAudited<T>`
`IHasModificationTime`, `IHasModifier<T>`, `IModificationAudited<T>`
`ISoftDelete`, `IHasDeletionTime`, `IHasDeleter<T>`, `IDeletionAudited<T>`
`IFullAudited<T>`

```csharp
public partial class Post : IFullAudited<Guid>
{
	public int Id { get; set; }
	public string Title { get; set; } = null!;
	...
}
```

