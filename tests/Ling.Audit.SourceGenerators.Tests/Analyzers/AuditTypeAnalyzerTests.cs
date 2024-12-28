using Ling.Audit.SourceGenerators.Diagnostics;
using Xunit;
using VerifyCS = Ling.Audit.SourceGenerators.Tests.Verifiers.CSharpAnalyzerVerifier<
    Ling.Audit.SourceGenerators.Analyzers.AuditTypeAnalyzer>;

namespace Ling.Audit.SourceGenerators.Tests.Analyzers;

public class AuditTypeAnalyzerTests
{
    #region Value Types

    [Fact]
    public async Task ValueType_WithAuditInterface_ReportsDiagnostic()
    {
        const string test = """
            using Ling.Audit;

            public struct MyStruct : IHasCreator<string>
            {
                public string CreatedBy { get; set; }
            }
            """;

        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.ValueType)
            .WithLocation(3, 15)
            .WithArguments("MyStruct");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task RecordStruct_WithAuditInterface_ReportsDiagnostic()
    {
        const string test = """
            using Ling.Audit;

            public record struct MyRecord : IHasCreator<string>
            {
                public string CreatedBy { get; set; }
            }
            """;

        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.ValueType)
            .WithLocation(3, 22)
            .WithArguments("MyRecord");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    #endregion

    #region Reference Types

    [Fact]
    public async Task Class_WithoutPartialKeyword_ReportsDiagnostic()
    {
        const string test = """
            using Ling.Audit;

            public class MyEntity : IHasCreator<string>
            {
            }
            """;

        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.PartialType)
            .WithLocation(3, 14)
            .WithArguments("MyEntity");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task Class_WithPartialKeyword_NoDiagnostic()
    {
        const string test = """
            using Ling.Audit;

            public partial class MyEntity : IHasCreator<string>
            {
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task Class_WithPartialKeywordAndMultipleInterfaces_NoDiagnostic()
    {
        const string test = """
            using Ling.Audit;
            using System;

            public partial class MyEntity : IHasCreator<string>, IHasCreationTime
            {
                public string CreatedBy { get; set; }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task Record_WithoutPartialKeyword_ReportsDiagnostic()
    {
        const string test = """
            using Ling.Audit;

            public record MyRecord : IHasCreator<string>
            {
            }
            """;

        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.PartialType)
            .WithLocation(3, 15)
            .WithArguments("MyRecord");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task Record_WithPartialKeyword_NoDiagnostic()
    {
        const string test = """
            using Ling.Audit;

            public partial record MyRecord : IHasCreator<string>
            {
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task Record_WithPartialKeywordAndMultipleInterfaces_NoDiagnostic()
    {
        const string test = """
            using Ling.Audit;
            using System;

            public partial record MyRecord : IHasCreator<string>, IHasCreationTime
            {
                public string CreatedBy { get; set; }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    #endregion
}
