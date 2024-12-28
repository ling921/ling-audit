using Ling.Audit.SourceGenerators.Diagnostics;
using Xunit;
using VerifyCS = Ling.Audit.SourceGenerators.Tests.Verifiers.CSharpCodeFixVerifier<
    Ling.Audit.SourceGenerators.Analyzers.MustNullAttributeAnalyzer,
    Ling.Audit.SourceGenerators.CodeFixes.MakeNullableCodeFixProvider>;

namespace Ling.Audit.SourceGenerators.Tests.CodeFixes;

public class MakeNullableCodeFixTests
{
    #region Value Type Parameters

    [Fact]
    public async Task ValueType_InField_FixAddsNullable()
    {
        const string source = """
            using Ling.Audit;

            public class Container<[MustNull] T>
            {
            }

            public class Usage
            {
                private Container<int> _field;
            }
            """;

        const string fixedSource = """
            using Ling.Audit;

            public class Container<[MustNull] T>
            {
            }

            public class Usage
            {
                private Container<int?> _field;
            }
            """;

        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.TypeParameterMustBeNullable)
            .WithLocation(9, 23)
            .WithArguments("int");

        await VerifyCS.VerifyCodeFixAsync(source, fixedSource, expected);
    }

    [Fact]
    public async Task ValueType_InProperty_FixAddsNullable()
    {
        const string source = """
            using Ling.Audit;

            public class Container<[MustNull] T>
            {
            }

            public class Usage
            {
                public Container<int> Property { get; set; }
            }
            """;

        const string fixedSource = """
            using Ling.Audit;

            public class Container<[MustNull] T>
            {
            }

            public class Usage
            {
                public Container<int?> Property { get; set; }
            }
            """;

        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.TypeParameterMustBeNullable)
            .WithLocation(9, 22)
            .WithArguments("int");

        await VerifyCS.VerifyCodeFixAsync(source, fixedSource, expected);
    }

    [Fact]
    public async Task ValueType_InMethodParameter_FixAddsNullable()
    {
        const string source = """
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

        const string fixedSource = """
            using Ling.Audit;

            public class Container<[MustNull] T>
            {
            }

            public class Usage
            {
                public void Method(Container<int?> param)
                {
                }
            }
            """;

        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.TypeParameterMustBeNullable)
            .WithLocation(9, 34)
            .WithArguments("int");

        await VerifyCS.VerifyCodeFixAsync(source, fixedSource, expected);
    }

    #endregion Value Type Parameters

    #region Nested Generic Types

    [Fact]
    public async Task ValueType_InNestedGeneric_FixAddsNullable()
    {
        const string source = """
            using System.Collections.Generic;
            using Ling.Audit;

            public class Container<[MustNull] T>
            {
            }

            public class Usage : Dictionary<string, Container<int>>
            {
            }
            """;

        const string fixedSource = """
            using System.Collections.Generic;
            using Ling.Audit;

            public class Container<[MustNull] T>
            {
            }

            public class Usage : Dictionary<string, Container<int?>>
            {
            }
            """;

        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.TypeParameterMustBeNullable)
            .WithLocation(8, 51)
            .WithArguments("int");

        await VerifyCS.VerifyCodeFixAsync(source, fixedSource, expected);
    }

    [Fact]
    public async Task ValueType_InMultipleConstrainedTypes_FixAddsNullable()
    {
        const string source = """
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

        const string fixedSource = """
            using System.Collections.Generic;
            using Ling.Audit;

            public class Container1<[MustNull] T>
            {
            }

            public class Container2<[MustNull] T>
            {
            }

            public class Usage : Dictionary<Container1<int?>, Container2<int?>>
            {
            }
            """;

        var expected1 = VerifyCS.Diagnostic(DiagnosticDescriptors.TypeParameterMustBeNullable)
            .WithLocation(12, 44)
            .WithArguments("int");
        var expected2 = VerifyCS.Diagnostic(DiagnosticDescriptors.TypeParameterMustBeNullable)
            .WithLocation(12, 61)
            .WithArguments("int");

        await VerifyCS.VerifyCodeFixAsync(source, fixedSource, expected1, expected2);
    }

    #endregion Nested Generic Types

    #region Type Constraints - No Fix Available

    [Fact]
    public async Task StructConstraint_NoFixAvailable()
    {
        const string source = """
            using Ling.Audit;

            public class Container<[MustNull] T> where T : struct
            {
            }
            """;

        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.TypeParameterMustBeNullable)
            .WithLocation(3, 35)
            .WithArguments("T");

        await VerifyCS.VerifyCodeFixAsync(source, source, expected);
    }

    [Fact]
    public async Task NotNullConstraint_NoFixAvailable()
    {
        const string source = """
            using Ling.Audit;

            public class Container<[MustNull] T> where T : notnull
            {
            }
            """;

        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.TypeParameterMustBeNullable)
            .WithLocation(3, 35)
            .WithArguments("T");

        await VerifyCS.VerifyCodeFixAsync(source, source, expected);
    }

    #endregion Type Constraints - No Fix Available

    #region Reference Types - No Fix Needed

    [Fact]
    public async Task ReferenceType_NoFixNeeded()
    {
        const string source = """
            using Ling.Audit;

            public class Container<[MustNull] T>
            {
            }

            public class Usage
            {
                private Container<string> _field;
            }
            """;

        // No diagnostic should be reported for reference types
        await VerifyCS.VerifyCodeFixAsync(source, source);
    }

    #endregion Reference Types - No Fix Needed

    #region Lambda Expressions

    [Fact]
    public async Task ValueType_InLambdaParameter_FixAddsNullable()
    {
        const string source = """
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

        const string fixedSource = """
            using System;
            using Ling.Audit;

            public class Container<[MustNull] T>
            {
            }

            public class Usage
            {
                public void Method()
                {
                    var func = (Container<int?> x) => x;
                }
            }
            """;

        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.TypeParameterMustBeNullable)
            .WithLocation(12, 31)
            .WithArguments("int");

        await VerifyCS.VerifyCodeFixAsync(source, fixedSource, expected);
    }

    #endregion Lambda Expressions
}
