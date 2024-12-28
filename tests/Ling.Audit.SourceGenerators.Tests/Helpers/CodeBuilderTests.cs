using Ling.Audit.SourceGenerators.Helpers;
using Xunit;

namespace Ling.Audit.SourceGenerators.Tests.Helpers;

public class CodeBuilderTests
{
    [Fact]
    public void Constructor_WithDefaultIndentSize_InitializesCorrectly()
    {
        var builder = new CodeBuilder();
        Assert.Equal(4, builder.IndentSize);
        Assert.Equal(0, builder.Length);
    }

    [Fact]
    public void Constructor_WithCustomIndentSize_InitializesCorrectly()
    {
        var builder = new CodeBuilder(2);
        Assert.Equal(2, builder.IndentSize);
        Assert.Equal(0, builder.Length);
    }

    [Fact]
    public void Constructor_WithNegativeIndentSize_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CodeBuilder(-1));
    }

    [Fact]
    public void Append_AppendsTextCorrectly()
    {
        var builder = new CodeBuilder();
        builder.Append("Hello");
        Assert.Equal("Hello", builder.ToString());
    }

    [Fact]
    public void AppendLine_AppendsTextWithNewLine()
    {
        var builder = new CodeBuilder();
        builder.AppendLine("Hello");
        Assert.Equal($"Hello{Environment.NewLine}", builder.ToString());
    }

    [Fact]
    public void OpenBrace_AppendsOpeningBraceAndIncreasesIndent()
    {
        var builder = new CodeBuilder();
        builder.OpenBrace();
        Assert.Equal($"{{{Environment.NewLine}}}{Environment.NewLine}", builder.ToString());
    }

    [Fact]
    public void CloseBrace_AppendsClosingBraceAndDecreasesIndent()
    {
        var builder = new CodeBuilder();
        builder.OpenBrace();
        builder.CloseBrace();
        Assert.Equal($"{{{Environment.NewLine}}}{Environment.NewLine}", builder.ToString());
    }

    [Fact]
    public void CloseBrace_WithTextAfterBrace_AppendsTextCorrectly()
    {
        var builder = new CodeBuilder();
        builder.OpenBrace();
        builder.CloseBrace(")");
        Assert.Equal($"{{{Environment.NewLine}}}){Environment.NewLine}", builder.ToString());
    }

    [Fact]
    public void CloseBrace_WithSemicolon_AppendsSemicolonCorrectly()
    {
        var builder = new CodeBuilder();
        builder.OpenBrace();
        builder.CloseBrace(null, true);
        Assert.Equal($"{{{Environment.NewLine}}};{Environment.NewLine}", builder.ToString());
    }

    [Fact]
    public void CloseAllBrace_ClosesAllOpenBraces()
    {
        var builder = new CodeBuilder();
        builder.OpenBrace();
        builder.OpenBrace();
        builder.CloseAllBrace();
        Assert.Equal($"{{{Environment.NewLine}    {{{Environment.NewLine}    }}{Environment.NewLine}}}{Environment.NewLine}", builder.ToString());
    }

    [Fact]
    public void AppendFormat_AppendsFormattedTextCorrectly()
    {
        var builder = new CodeBuilder();
        builder.AppendFormat("Hello {0}", "World");
        Assert.Equal("Hello World", builder.ToString());
    }

    [Fact]
    public void AppendFormatLine_AppendsFormattedTextWithNewLine()
    {
        var builder = new CodeBuilder();
        builder.AppendFormatLine("Hello {0}", "World");
        Assert.Equal($"Hello World{Environment.NewLine}", builder.ToString());
    }

    [Fact]
    public void IncreaseIndentLevel_IncreasesIndentLevel()
    {
        var builder = new CodeBuilder();
        builder.IncreaseIndentLevel();
        builder.Append("Indented");
        Assert.Equal($"    Indented}}{Environment.NewLine}", builder.ToString());
    }

    [Fact]
    public void DecreaseIndentLevel_DecreasesIndentLevel()
    {
        var builder = new CodeBuilder();
        builder.IncreaseIndentLevel();
        builder.DecreaseIndentLevel();
        builder.Append("Not Indented");
        Assert.Equal("Not Indented", builder.ToString());
    }

    [Fact]
    public void Clear_ClearsTheBuilder()
    {
        var builder = new CodeBuilder();
        builder.Append("Hello");
        builder.Clear();
        Assert.Equal(0, builder.Length);
        Assert.Equal(string.Empty, builder.ToString());
    }

    [Fact]
    public void ToString_ClosesAllBracesAndReturnsString()
    {
        var builder = new CodeBuilder();
        builder.OpenBrace();
        builder.OpenBrace();
        var result = builder.ToString();
        Assert.Equal($"{{{Environment.NewLine}    {{{Environment.NewLine}    }}{Environment.NewLine}}}{Environment.NewLine}", result);
    }
}
