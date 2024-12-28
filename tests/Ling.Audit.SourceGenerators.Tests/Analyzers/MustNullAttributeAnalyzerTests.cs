using Ling.Audit.SourceGenerators.Diagnostics;
using Xunit;
using VerifyCS = Ling.Audit.SourceGenerators.Tests.Verifiers.CSharpAnalyzerVerifier<
    Ling.Audit.SourceGenerators.Analyzers.MustNullAttributeAnalyzer>;

namespace Ling.Audit.SourceGenerators.Tests.Analyzers;

public class MustNullAttributeAnalyzerTests
{
    #region Type Parameter Constraints

    [Fact]
    public async Task TypeParameter_WithoutConstraint_NoDiagnostic()
    {
        const string test = """
            using Ling.Audit;

            public class Container<[MustNull] T>
            {
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task TypeParameter_WithReferenceTypeConstraint_NoDiagnostic()
    {
        const string test = """
            using Ling.Audit;

            public class Container<[MustNull] T> where T : class
            {
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task TypeParameter_WithValueTypeConstraint_ReportsDiagnostic()
    {
        const string test = """
            using Ling.Audit;

            public class Container<[MustNull] T> where T : struct
            {
            }
            """;

        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.TypeParameterMustBeNullable)
            .WithLocation(3, 35)
            .WithArguments("T");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task TypeParameter_WithNotNullConstraint_ReportsDiagnostic()
    {
        const string test = """
            using Ling.Audit;

            public class Container<[MustNull] T> where T : notnull
            {
            }
            """;

        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.TypeParameterMustBeNullable)
            .WithLocation(3, 35)
            .WithArguments("T");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    #endregion Type Parameter Constraints

    #region Inheritance Chain

    [Fact]
    public async Task Inheritance_WithReferenceType_NoDiagnostic()
    {
        const string test = """
            using Ling.Audit;

            public class Container<[MustNull] T>
            {
            }

            public class Derived : Container<string>
            {
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task Inheritance_WithValueType_ReportsDiagnostic()
    {
        const string test = """
            using Ling.Audit;

            public class Container<[MustNull] T>
            {
            }

            public class Derived : Container<int>
            {
            }
            """;

        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.TypeParameterMustBeNullable)
            .WithLocation(7, 34)
            .WithArguments("int");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task Inheritance_WithMultipleParameters_ReportsDiagnostic()
    {
        const string test = """
            using Ling.Audit;

            public class Container<T1, [MustNull] T2, T3>
            {
            }

            public class Derived : Container<string, int, bool>
            {
            }
            """;

        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.TypeParameterMustBeNullable)
            .WithLocation(7, 42)
            .WithArguments("int");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    #endregion Inheritance Chain

    #region Member Declarations

    [Fact]
    public async Task Field_WithValueType_ReportsDiagnostic()
    {
        const string test = """
            using Ling.Audit;

            public class Container<[MustNull] T>
            {
            }

            public class Usage
            {
                private Container<int> _field;
            }
            """;

        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.TypeParameterMustBeNullable)
            .WithLocation(9, 23)
            .WithArguments("int");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task Property_WithValueType_ReportsDiagnostic()
    {
        const string test = """
            using Ling.Audit;

            public class Container<[MustNull] T>
            {
            }

            public class Usage
            {
                public Container<int> Property { get; set; }
            }
            """;

        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.TypeParameterMustBeNullable)
            .WithLocation(9, 22)
            .WithArguments("int");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    #endregion Member Declarations

    #region Nested Generic Types

    [Fact]
    public async Task NestedGeneric_WithValueType_ReportsDiagnostic()
    {
        const string test = """
            using System.Collections.Generic;
            using Ling.Audit;

            public class Container<[MustNull] T>
            {
            }

            public class Usage : List<Container<int>>
            {
            }
            """;

        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.TypeParameterMustBeNullable)
            .WithLocation(8, 37)
            .WithArguments("int");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task NestedGeneric_WithMultipleParameters_ReportsDiagnostic()
    {
        const string test = """
            using System.Collections.Generic;
            using Ling.Audit;

            public class Container<[MustNull] T>
            {
            }

            public class Usage : Dictionary<string, Container<int>>
            {
            }
            """;

        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.TypeParameterMustBeNullable)
            .WithLocation(8, 51)
            .WithArguments("int");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    #endregion Nested Generic Types

    #region Delegate Types

    [Fact]
    public async Task Delegate_WithValueType_ReportsDiagnostic()
    {
        const string test = """
            using Ling.Audit;

            public class Container<[MustNull] T>
            {
            }

            public delegate Container<int> MyDelegate(Container<int> param);
            """;

        var expected1 = VerifyCS.Diagnostic(DiagnosticDescriptors.TypeParameterMustBeNullable)
            .WithLocation(7, 27)
            .WithArguments("int");
        var expected2 = VerifyCS.Diagnostic(DiagnosticDescriptors.TypeParameterMustBeNullable)
            .WithLocation(7, 53)
            .WithArguments("int");

        await VerifyCS.VerifyAnalyzerAsync(test, expected1, expected2);
    }

    [Fact]
    public async Task DelegateVariable_WithValueType_ReportsDiagnostic()
    {
        const string test = """
            using System;
            using Ling.Audit;

            public class Container<[MustNull] T>
            {
            }

            public class Usage
            {
                private Action<Container<int>> _action = x => { };
            }
            """;

        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.TypeParameterMustBeNullable)
            .WithLocation(10, 30)
            .WithArguments("int");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task LambdaParameter_WithValueType_ReportsDiagnostic()
    {
        const string test = """
            using System;
            using Ling.Audit;

            public class Container<[MustNull] T>
            {
            }

            public class Usage
            {
                private Func<Container<int>, bool> _func = x => true;
            }
            """;

        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.TypeParameterMustBeNullable)
            .WithLocation(10, 28)
            .WithArguments("int");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    #endregion Delegate Types

    #region Complex Inheritance Chain

    [Fact]
    public async Task Inheritance_DeepChain_ReportsDiagnostic()
    {
        const string test = """
            using Ling.Audit;

            public class Base<[MustNull] T>
            {
            }

            public class Middle<U> : Base<U>
            {
            }

            public class Derived : Middle<int>
            {
            }
            """;

        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.TypeParameterMustBeNullable)
            .WithLocation(11, 31)
            .WithArguments("int");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task Inheritance_MultipleInterfaces_ReportsDiagnostic()
    {
        const string test = """
            using Ling.Audit;

            public interface IBase<[MustNull] T>
            {
            }

            public interface IOther<U>
            {
            }

            public class Derived : IOther<string>, IBase<int>
            {
            }
            """;

        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.TypeParameterMustBeNullable)
            .WithLocation(11, 46)
            .WithArguments("int");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    #endregion Complex Inheritance Chain

    #region Complex Nested Types

    [Fact]
    public async Task NestedGeneric_MultipleLayersDeep_ReportsDiagnostic()
    {
        const string test = """
            using System.Collections.Generic;
            using Ling.Audit;

            public class Container<[MustNull] T>
            {
            }

            public class Usage : Dictionary<string, List<Container<int>>>
            {
            }
            """;

        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.TypeParameterMustBeNullable)
            .WithLocation(8, 56)
            .WithArguments("int");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task NestedGeneric_MultipleConstrainedTypes_ReportsDiagnostic()
    {
        const string test = """
            using System.Collections.Generic;
            using Ling.Audit;

            public class Container1<[MustNull] T>
            {
            }

            public class Container2<[MustNull] T>
            {
            }

            public class Usage : Dictionary<Container1<int>, Container2<int>>
            {
            }
            """;

        var expected1 = VerifyCS.Diagnostic(DiagnosticDescriptors.TypeParameterMustBeNullable)
            .WithLocation(12, 44)
            .WithArguments("int");
        var expected2 = VerifyCS.Diagnostic(DiagnosticDescriptors.TypeParameterMustBeNullable)
            .WithLocation(12, 61)
            .WithArguments("int");

        await VerifyCS.VerifyAnalyzerAsync(test, expected1, expected2);
    }

    #endregion Complex Nested Types

    #region Method Parameters and Return Types

    [Fact]
    public async Task Method_ParameterType_ReportsDiagnostic()
    {
        const string test = """
            using Ling.Audit;

            public class Container<[MustNull] T>
            {
            }

            public class Usage
            {
                public void Method(Container<int> param)
                {
                }
            }
            """;

        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.TypeParameterMustBeNullable)
            .WithLocation(9, 34)
            .WithArguments("int");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task Method_ReturnType_ReportsDiagnostic()
    {
        const string test = """
            using Ling.Audit;

            public class Container<[MustNull] T>
            {
            }

            public class Usage
            {
                public Container<int> Method() => null;
            }
            """;

        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.TypeParameterMustBeNullable)
            .WithLocation(9, 22)
            .WithArguments("int");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    #endregion Method Parameters and Return Types

    #region Lambda Expressions

    [Fact]
    public async Task Lambda_AsMethodArgument_ReportsDiagnostic()
    {
        const string test = """
            using System;
            using Ling.Audit;

            public class Container<[MustNull] T>
            {
            }

            public class Usage
            {
                public void Process(Action<Container<int>> action)
                {
                }

                public void Method()
                {
                    Process(x => { });
                }
            }
            """;

        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.TypeParameterMustBeNullable)
            .WithLocation(10, 42)
            .WithArguments("int");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task Lambda_AsPropertyInitializer_ReportsDiagnostic()
    {
        const string test = """
            using System;
            using Ling.Audit;

            public class Container<[MustNull] T>
            {
            }

            public class Usage
            {
                public Action<Container<int>> Handler { get; } = x => { };
            }
            """;

        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.TypeParameterMustBeNullable)
            .WithLocation(10, 29)
            .WithArguments("int");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task Lambda_WithExplicitParameterType_ReportsDiagnostic()
    {
        const string test = """
            using System;
            using Ling.Audit;

            public class Container<[MustNull] T>
            {
            }

            public class Usage
            {
                public void Method()
                {
                    var func = (Container<int> x) => x;
                }
            }
            """;

        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.TypeParameterMustBeNullable)
            .WithLocation(12, 31)
            .WithArguments("int");

        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    #endregion Lambda Expressions
}
