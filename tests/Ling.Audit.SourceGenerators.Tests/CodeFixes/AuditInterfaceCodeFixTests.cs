using Ling.Audit.SourceGenerators.Diagnostics;
using Xunit;
using VerifyCS = Ling.Audit.SourceGenerators.Tests.Verifiers.CSharpCodeFixVerifier<
    Ling.Audit.SourceGenerators.Analyzers.AuditInterfaceAnalyzer,
    Ling.Audit.SourceGenerators.CodeFixes.AuditInterfaceCodeFixProvider>;

namespace Ling.Audit.SourceGenerators.Tests.CodeFixes;

public class AuditInterfaceCodeFixTests
{
    [Fact]
    public async Task UseCreationAuditedInterface_FixesCode()
    {
        const string test = """
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

        await VerifyCS.VerifyCodeFixAsync(test, fixedCode, expected);
    }

    [Fact]
    public async Task UseModificationAuditedInterface_FixesCode()
    {
        const string test = """
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

        await VerifyCS.VerifyCodeFixAsync(test, fixedCode, expected);
    }

    [Fact]
    public async Task UseDeletionAuditedInterface_FixesCode()
    {
        const string test = """
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

        await VerifyCS.VerifyCodeFixAsync(test, fixedCode, expected);
    }

    [Fact]
    public async Task UseFullAuditedInterface_FixesCode()
    {
        const string test = """
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

        await VerifyCS.VerifyCodeFixAsync(test, fixedCode, expected);
    }
}
