using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;

namespace Ling.Audit.SourceGenerators.Tests.Verifiers;

internal static partial class CSharpSourceGeneratorVerifier<TSourceGenerator>
    where TSourceGenerator : new()
{
    [GeneratedRegex(@"\r?\n")]
    private static partial Regex NewLineRegex();

    public static async Task VerifySourceGeneratorAsync(string source, params (string FileName, string GeneratedCode)[] generatedSources)
    {
        var test = new Test
        {
            TestState =
            {
                Sources = { source },
            },
        };

        foreach (var (fileName, generatedCode) in generatedSources)
        {
            var sourceText = NewLineRegex().Replace(generatedCode, Environment.NewLine);

            test.TestState.GeneratedSources.Add((typeof(TSourceGenerator), fileName, SourceText.From(sourceText, Encoding.UTF8)));
        }

        await test.RunAsync();
    }

    private class Test : CSharpSourceGeneratorTest<TSourceGenerator, DefaultVerifier>
    {
        protected override string DefaultTestProjectName => "Ling.Audit.Tests";

        public Test()
        {
            TestState.AdditionalReferences.Add(typeof(ISoftDelete).Assembly);
            TestState.ReferenceAssemblies = ReferenceAssemblies.Net.Net80;
        }

        protected override ImmutableArray<(Project project, Diagnostic diagnostic)> FilterDiagnostics(ImmutableArray<(Project project, Diagnostic diagnostic)> diagnostics)
        {
            return diagnostics.Where(d => d.diagnostic.Id.StartsWith("LA")).ToImmutableArray();
        }
    }
}
