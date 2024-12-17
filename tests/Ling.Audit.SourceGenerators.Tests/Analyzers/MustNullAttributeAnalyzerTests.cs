using Xunit;
using Ling.Audit.SourceGenerators.Diagnostics;
using VerifyCS = Ling.Audit.SourceGenerators.Tests.Verifiers.CSharpAnalyzerVerifier<Ling.Audit.SourceGenerators.Analyzers.MustNullAttributeAnalyzer>;

namespace Ling.Audit.SourceGenerators.Tests.Analyzers;

public class MustNullAttributeAnalyzerTests
{
    [Fact]
    public async Task TypeParameter_WithValueTypeAndNotNullConstraint_ReportsDiagnostic()
    {
        var test = @"
using Ling.Audit;

public class Test<[MustNull] T> where T : struct { }
";
        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.TypeParameterMustBeNullable)
            .WithLocation(3, 24)
            .WithArguments("T");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task GenericType_WithValueTypeArgument_ReportsDiagnostic()
    {
        var test = @"
using Ling.Audit;

public class Container<[MustNull] T> { }

public class Usage
{
    private Container<int> field;
}";

        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.TypeParameterMustBeNullable)
            .WithLocation(7, 23)
            .WithArguments("T");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task GenericType_WithNullableValueType_NoDiagnostic()
    {
        var test = @"
using Ling.Audit;

public class Container<[MustNull] T> { }

public class Usage
{
    private Container<int?> field;
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task Property_WithValueTypeArgument_ReportsDiagnostic()
    {
        var test = @"
using Ling.Audit;

public class Container<[MustNull] T> { }

public class Usage
{
    public Container<int> Property { get; set; }
}";

        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.TypeParameterMustBeNullable)
            .WithLocation(7, 23)
            .WithArguments("T");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task BaseType_WithValueTypeArgument_ReportsDiagnostic()
    {
        var test = @"
using Ling.Audit;

public class BaseContainer<[MustNull] T> { }

public class Derived : BaseContainer<int>
{
}";

        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.TypeParameterMustBeNullable)
            .WithLocation(5, 31)
            .WithArguments("T");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task MultipleTypeParameters_OnlyAnnotatedParameter_ReportsDiagnostic()
    {
        var test = @"
using Ling.Audit;

public class Container<T1, [MustNull] T2, T3> { }

public class Usage
{
    private Container<string, int, bool> field;
}";

        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.TypeParameterMustBeNullable)
            .WithLocation(7, 29)
            .WithArguments("T2");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task LocalVariable_WithValueTypeArgument_ReportsDiagnostic()
    {
        var test = @"
using Ling.Audit;

public class Container<[MustNull] T> { }

public class Usage
{
    public void Method()
    {
        Container<int> local = new Container<int>();
    }
}";

        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.TypeParameterMustBeNullable)
            .WithLocation(9, 18)
            .WithArguments("T");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task GenericType_WithReferenceType_NoDiagnostic()
    {
        var test = @"
using Ling.Audit;

public class Container<[MustNull] T> { }

public class Usage
{
    private Container<string> field;
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task TypeParameter_WithoutConstraint_NoDiagnostic()
    {
        var test = @"
using Ling.Audit;

public class Test<[MustNull] T> { }
";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task TypeParameter_WithReferenceTypeConstraint_NoDiagnostic()
    {
        var test = @"
using Ling.Audit;

public class Test<[MustNull] T> where T : class { }
";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }
}
