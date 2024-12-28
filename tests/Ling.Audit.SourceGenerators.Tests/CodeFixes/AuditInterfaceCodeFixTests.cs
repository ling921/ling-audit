using Ling.Audit.SourceGenerators.Diagnostics;
using Xunit;
using VerifyCS = Ling.Audit.SourceGenerators.Tests.Verifiers.CSharpCodeFixVerifier<
    Ling.Audit.SourceGenerators.Analyzers.AuditInterfaceAnalyzer,
    Ling.Audit.SourceGenerators.CodeFixes.AuditInterfaceCodeFixProvider>;

namespace Ling.Audit.SourceGenerators.Tests.CodeFixes;

public class AuditInterfaceCodeFixTests
{
    [Fact]
    public async Task ReplaceCreationAuditInterfaces_WithCreationAudited()
    {
        const string source = """
            using Ling.Audit;
            using System;

            public partial class MyEntity : IHasCreator<string>, IHasCreationTime
            {
                public string? CreatedBy { get; set; }
                public DateTimeOffset CreatedAt { get; set; }
            }
            """;

        const string fixedCode = """
            using Ling.Audit;
            using System;

            public partial class MyEntity : ICreationAudited<string>
            {
                public string? CreatedBy { get; set; }
                public DateTimeOffset CreatedAt { get; set; }
            }
            """;

        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.UseAuditedInterface)
            .WithLocation(4, 54)
            .WithArguments("ICreationAudited<TKey>");

        await VerifyCS.VerifyCodeFixAsync(source, fixedCode, expected);
    }

    [Fact]
    public async Task ReplaceModificationAuditInterfaces_WithModificationAudited()
    {
        const string source = """
            using Ling.Audit;
            using System;

            public partial class MyEntity : IHasModifier<string>, IHasModificationTime
            {
                public string? LastModifiedBy { get; set; }
                public DateTimeOffset? LastModifiedAt { get; set; }
            }
            """;

        const string fixedCode = """
            using Ling.Audit;
            using System;

            public partial class MyEntity : IModificationAudited<string>
            {
                public string? LastModifiedBy { get; set; }
                public DateTimeOffset? LastModifiedAt { get; set; }
            }
            """;

        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.UseAuditedInterface)
            .WithLocation(4, 55)
            .WithArguments("IModificationAudited<TKey>");

        await VerifyCS.VerifyCodeFixAsync(source, fixedCode, expected);
    }

    [Fact]
    public async Task ReplaceDeletionAuditInterfaces_WithDeletionAudited()
    {
        const string source = """
            using Ling.Audit;
            using System;

            public partial class MyEntity : ISoftDelete, IHasDeleter<string>, IHasDeletionTime
            {
                public bool IsDeleted { get; set; }
                public string? DeletedBy { get; set; }
                public DateTimeOffset? DeletedAt { get; set; }
            }
            """;

        const string fixedCode = """
            using Ling.Audit;
            using System;

            public partial class MyEntity : IDeletionAudited<string>
            {
                public bool IsDeleted { get; set; }
                public string? DeletedBy { get; set; }
                public DateTimeOffset? DeletedAt { get; set; }
            }
            """;

        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.UseAuditedInterface)
            .WithLocation(4, 67)
            .WithArguments("IDeletionAudited<TKey>");

        await VerifyCS.VerifyCodeFixAsync(source, fixedCode, expected);
    }

    [Fact]
    public async Task ReplaceAllAuditInterfaces_WithFullAudited()
    {
        const string source = """
            using Ling.Audit;
            using System;

            public partial class MyEntity : ICreationAudited<string>, IModificationAudited<string>, IDeletionAudited<string>
            {
                public string? CreatedBy { get; set; }
                public DateTimeOffset CreatedAt { get; set; }
                public string? LastModifiedBy { get; set; }
                public DateTimeOffset? LastModifiedAt { get; set; }
                public bool IsDeleted { get; set; }
                public string? DeletedBy { get; set; }
                public DateTimeOffset? DeletedAt { get; set; }
            }
            """;

        const string fixedCode = """
            using Ling.Audit;
            using System;

            public partial class MyEntity : IFullAudited<string>
            {
                public string? CreatedBy { get; set; }
                public DateTimeOffset CreatedAt { get; set; }
                public string? LastModifiedBy { get; set; }
                public DateTimeOffset? LastModifiedAt { get; set; }
                public bool IsDeleted { get; set; }
                public string? DeletedBy { get; set; }
                public DateTimeOffset? DeletedAt { get; set; }
            }
            """;

        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.UseAuditedInterface)
            .WithLocation(4, 89)
            .WithArguments("IFullAudited<TKey>");

        await VerifyCS.VerifyCodeFixAsync(source, fixedCode, expected);
    }

    [Fact]
    public async Task PreserveOtherInterfaces_WhenReplacingAuditInterfaces()
    {
        const string source = """
            using Ling.Audit;
            using System;

            public partial class MyEntity : IHasCreator<string>, IHasCreationTime, IEntity
            {
                public string? CreatedBy { get; set; }
                public DateTimeOffset CreatedAt { get; set; }
            }

            public interface IEntity { }
            """;

        const string fixedCode = """
            using Ling.Audit;
            using System;

            public partial class MyEntity : ICreationAudited<string>, IEntity
            {
                public string? CreatedBy { get; set; }
                public DateTimeOffset CreatedAt { get; set; }
            }
            
            public interface IEntity { }
            """;

        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.UseAuditedInterface)
            .WithLocation(4, 54)
            .WithArguments("ICreationAudited<TKey>");

        await VerifyCS.VerifyCodeFixAsync(source, fixedCode, expected);
    }

    [Fact]
    public async Task HandleMultilineInterfaceList_WhenReplacingInterfaces()
    {
        const string source = """
            using Ling.Audit;
            using System;

            public partial class MyEntity :
                IHasCreator<string>,
                IHasCreationTime,
                ISoftDelete,
                IHasDeleter<string>,
                IHasDeletionTime,
                IEntity
            {
            }

            public interface IEntity { }
            """;

        const string fixedCode = """
            using Ling.Audit;
            using System;

            public partial class MyEntity :
                ICreationAudited<string>,
                IDeletionAudited<string>,
                IEntity
            {
            }
            
            public interface IEntity { }
            """;

        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.UseAuditedInterface)
            .WithLocation(6, 5)
            .WithArguments("ICreationAudited<TKey>");
        var expected2 = VerifyCS.Diagnostic(DiagnosticDescriptors.UseAuditedInterface)
            .WithLocation(9, 5)
            .WithArguments("IDeletionAudited<TKey>");

        await VerifyCS.VerifyCodeFixAsync(source, fixedCode, 2, expected, expected2);
    }

    [Fact]
    public async Task HandleFullyQualifiedNames_WhenReplacingInterfaces()
    {
        const string source = """
            using System;

            public partial class MyEntity :
                Ling.Audit.ICreationAudited<string>,
                Ling.Audit.ISoftDelete,
                Ling.Audit.IHasDeleter<string>,
                Ling.Audit.IHasDeletionTime
            {
            }
            """;

        const string fixedCode = """
            using System;

            public partial class MyEntity :
                Ling.Audit.ICreationAudited<string>,
                Ling.Audit.IDeletionAudited<string>
            {
            }
            """;

        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.UseAuditedInterface)
            .WithLocation(7, 5)
            .WithArguments("IDeletionAudited<TKey>");

        await VerifyCS.VerifyCodeFixAsync(source, fixedCode, expected);
    }

    [Fact]
    public async Task HandleCrossedAuditInterfaces_WhenReplacingInterfaces()
    {
        const string source = """
            using Ling.Audit;
            using System;

            public partial class MyEntity :
                IHasCreator<string>,
                IHasModificationTime,
                IHasCreationTime,
                IHasModifier<string>
            {
                public string? CreatedBy { get; set; }
                public DateTimeOffset CreatedAt { get; set; }
                public string? LastModifiedBy { get; set; }
                public DateTimeOffset? LastModifiedAt { get; set; }
            }
            """;

        const string fixedCode = """
            using Ling.Audit;
            using System;

            public partial class MyEntity :
                ICreationAudited<string>,
                IModificationAudited<string>
            {
                public string? CreatedBy { get; set; }
                public DateTimeOffset CreatedAt { get; set; }
                public string? LastModifiedBy { get; set; }
                public DateTimeOffset? LastModifiedAt { get; set; }
            }
            """;

        var expected1 = VerifyCS.Diagnostic(DiagnosticDescriptors.UseAuditedInterface)
            .WithLocation(7, 5)
            .WithArguments("ICreationAudited<TKey>");
        var expected2 = VerifyCS.Diagnostic(DiagnosticDescriptors.UseAuditedInterface)
            .WithLocation(8, 5)
            .WithArguments("IModificationAudited<TKey>");

        await VerifyCS.VerifyCodeFixAsync(source, fixedCode, 2, expected1, expected2);
    }
}
