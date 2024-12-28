using Ling.Audit.SourceGenerators.Diagnostics;
using Xunit;
using VerifyCS = Ling.Audit.SourceGenerators.Tests.Verifiers.CSharpAnalyzerVerifier<
    Ling.Audit.SourceGenerators.Analyzers.AuditKeyTypeAnalyzer>;

namespace Ling.Audit.SourceGenerators.Tests.Analyzers;

public class AuditKeyTypeAnalyzerTests
{
    [Fact]
    public async Task Class_WithMismatchedKeyTypes_ReportsDiagnostic()
    {
        const string test = """
            using Ling.Audit;
            using System;

            public class MyEntity : IHasCreator<int>, IHasModifier<string>
            {
                public int CreatedBy { get; set; }
                public string LastModifiedBy { get; set; }
            }
            """;

        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.KeyTypeMismatch)
            .WithLocation(4, 14)
            .WithArguments("MyEntity", "int, string");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task Class_WithMatchingKeyTypes_NoDiagnostic()
    {
        const string test = """
            using Ling.Audit;
            using System;

            public class MyEntity : IHasCreator<int>, IHasLastModifier<int>
            {
                public int CreatedBy { get; set; }
                public int LastModifiedBy { get; set; }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task Class_WithSingleAuditInterface_NoDiagnostic()
    {
        const string test = """
            using Ling.Audit;
            using System;

            public class MyEntity : IHasCreator<int>
            {
                public int CreatedBy { get; set; }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task Record_WithMismatchedKeyTypes_ReportsDiagnostic()
    {
        const string test = """
            using Ling.Audit;
            using System;

            public record MyRecord : IHasCreator<int>, IHasModifier<string>
            {
                public int CreatedBy { get; set; }
                public string LastModifiedBy { get; set; }
            }
            """;

        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.KeyTypeMismatch)
            .WithLocation(4, 15)
            .WithArguments("MyRecord", "int, string");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task Record_WithMatchingKeyTypes_NoDiagnostic()
    {
        const string test = """
            using Ling.Audit;
            using System;

            public record MyRecord : IHasCreator<int>, IHasLastModifier<int>
            {
                public int CreatedBy { get; set; }
                public int LastModifiedBy { get; set; }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(test);
    }
}
