using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using System.Collections.Immutable;

namespace Ling.Audit.SourceGenerators.Tests.Verifiers;

internal static class CSharpCodeFixVerifier<TAnalyzer, TCodeFix>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TCodeFix : CodeFixProvider, new()
{
    public static DiagnosticResult Diagnostic()
        => CSharpAnalyzerVerifier<TAnalyzer, DefaultVerifier>.Diagnostic();

    public static DiagnosticResult Diagnostic(string diagnosticId)
        => CSharpAnalyzerVerifier<TAnalyzer, DefaultVerifier>.Diagnostic(diagnosticId);

    public static DiagnosticResult Diagnostic(DiagnosticDescriptor descriptor)
        => CSharpAnalyzerVerifier<TAnalyzer, DefaultVerifier>.Diagnostic(descriptor);

    public static async Task VerifyCodeFixAsync(string source, string fixedSource, params DiagnosticResult[] expected)
    {
        var test = new Test
        {
            TestCode = source,
            FixedCode = fixedSource,
        };

        test.ExpectedDiagnostics.AddRange(expected);
        await test.RunAsync(CancellationToken.None);
    }

    private class Test : CSharpCodeFixTest<TAnalyzer, TCodeFix, DefaultVerifier>
    {
        public Test()
        {
            TestState.AdditionalReferences.Add(typeof(MustNullAttribute).Assembly);
            TestState.ReferenceAssemblies = ReferenceAssemblies.Net.Net80;
        }

        protected override ImmutableArray<(Project project, Diagnostic diagnostic)> FilterDiagnostics(ImmutableArray<(Project project, Diagnostic diagnostic)> diagnostics)
        {
            return diagnostics.Where(d => d.diagnostic.Id.StartsWith("LA")).ToImmutableArray();
        }
    }
}
