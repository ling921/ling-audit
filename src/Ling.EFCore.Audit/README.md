### Introduction

Ling.EFCore.Audit is an extension library that can automatically record entity changes of `Microsoft.EntityFrameworkCore`.

### Installation

First, [install NuGet](http://docs.nuget.org/docs/start-here/installing-nuget). Then, install [Ling.EFCore.Audit](https://www.nuget.org/packages/Ling.EFCore.Audit/) from the package manager console:

```
PM> Install-Package Ling.EFCore.Audit
```
Or from the .NET CLI as:
```
dotnet add package Ling.EFCore.Audit
```

### Usage

Add `UseAudit()` in your `DbContext` service registration code.

```csharp
// in Program.cs
builder.Services.Addxxx<xxDbContext>(
    connectionString,
    optionsAction: options => options.UseAudit());

// in Startup.cs
services.Addxxx<xxDbContext>(
    connectionString,
    optionsAction: options => options.UseAudit());
```
