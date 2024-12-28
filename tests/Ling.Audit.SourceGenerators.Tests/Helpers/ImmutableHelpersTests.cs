using Ling.Audit.SourceGenerators.Helpers;
using System.Collections.Immutable;
using Xunit;

namespace Ling.Audit.SourceGenerators.Tests.Helpers;

public class ImmutableHelpersTests
{
    [Fact]
    public void FindIndex_WithMatchingElement_ReturnsCorrectIndex()
    {
        var array = ImmutableArray.Create(1, 2, 3, 4, 5);
        var index = array.FindIndex(x => x == 3);
        Assert.Equal(2, index);
    }

    [Fact]
    public void FindIndex_WithNoMatchingElement_ReturnsMinusOne()
    {
        var array = ImmutableArray.Create(1, 2, 3, 4, 5);
        var index = array.FindIndex(x => x == 6);
        Assert.Equal(-1, index);
    }

    [Fact]
    public void FindIndex_WithEmptyArray_ReturnsMinusOne()
    {
        var array = ImmutableArray<int>.Empty;
        var index = array.FindIndex(x => x == 1);
        Assert.Equal(-1, index);
    }

    [Fact]
    public void FindIndex_WithNullPredicate_ThrowsArgumentNullException()
    {
        var array = ImmutableArray.Create(1, 2, 3, 4, 5);
        Assert.Throws<ArgumentNullException>(() => array.FindIndex(null!));
    }

    [Fact]
    public void FindIndex_WithMultipleMatchingElements_ReturnsFirstMatchIndex()
    {
        var array = ImmutableArray.Create(1, 2, 3, 4, 3, 5);
        var index = array.FindIndex(x => x == 3);
        Assert.Equal(2, index);
    }
}
