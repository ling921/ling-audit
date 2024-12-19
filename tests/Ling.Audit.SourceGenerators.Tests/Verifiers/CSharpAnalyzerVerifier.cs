using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using System.Collections.Immutable;

namespace Ling.Audit.SourceGenerators.Tests.Verifiers;

internal static class CSharpAnalyzerVerifier<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    public static DiagnosticResult Diagnostic()
        => CSharpAnalyzerVerifier<TAnalyzer, DefaultVerifier>.Diagnostic();

    public static DiagnosticResult Diagnostic(string diagnosticId)
        => CSharpAnalyzerVerifier<TAnalyzer, DefaultVerifier>.Diagnostic(diagnosticId);

    public static DiagnosticResult Diagnostic(DiagnosticDescriptor descriptor)
        => CSharpAnalyzerVerifier<TAnalyzer, DefaultVerifier>.Diagnostic(descriptor);

    public static async Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
    {
        var test = new Test { TestCode = source };
        test.ExpectedDiagnostics.AddRange(expected);
        await test.RunAsync();
    }

    public static async Task VerifyAnalyzerAsync(string source, Type[] sourceGenerators, params DiagnosticResult[] expected)
    {
        var test = new Test { TestCode = source, TestBehaviors = TestBehaviors.SkipGeneratedCodeCheck };
        test.ExpectedDiagnostics.AddRange(expected);
        test.SourceGeneratorTypes.AddRange(sourceGenerators);
        await test.RunAsync();
    }

    public static async Task VerifyAnalyzerAsync(string source, List<string> disabledDiagnostics, params DiagnosticResult[] expected)
    {
        var test = new Test { TestCode = source };
        test.ExpectedDiagnostics.AddRange(expected);
        test.DisabledDiagnostics.AddRange(disabledDiagnostics);
        await test.RunAsync();
    }

    private class Test : CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
    {
        public List<Type> SourceGeneratorTypes = [];

        public Test()
        {
            TestState.AdditionalReferences.Add(typeof(MustNullAttribute).Assembly);
            TestState.ReferenceAssemblies = ReferenceAssemblies.Net.Net80;
        }

        protected override IEnumerable<Type> GetSourceGenerators()
        {
            return SourceGeneratorTypes;
        }

        protected override ImmutableArray<(Project project, Diagnostic diagnostic)> FilterDiagnostics(ImmutableArray<(Project project, Diagnostic diagnostic)> diagnostics)
        {
            return diagnostics.Where(d => d.diagnostic.Id.StartsWith("LA")).ToImmutableArray();
        }
    }
}
