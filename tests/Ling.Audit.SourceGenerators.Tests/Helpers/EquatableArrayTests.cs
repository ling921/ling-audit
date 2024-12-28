using Ling.Audit.SourceGenerators.Helpers;
using Xunit;

namespace Ling.Audit.SourceGenerators.Tests.Helpers;

public class EquatableArrayTests
{
    [Fact]
    public void Constructor_WithArray_InitializesCorrectly()
    {
        var array = new[] { 1, 2, 3 };
        var equatableArray = new EquatableArray<int>(array);

        Assert.Equal(3, equatableArray.Count);
        Assert.Equal(3, equatableArray.Length);
    }

    [Fact]
    public void Constructor_WithCollection_InitializesCorrectly()
    {
        var collection = new List<int> { 1, 2, 3 };
        var equatableArray = new EquatableArray<int>(collection);

        Assert.Equal(3, equatableArray.Count);
        Assert.Equal(3, equatableArray.Length);
    }

    [Fact]
    public void Empty_ReturnsEmptyArray()
    {
        var emptyArray = EquatableArray<int>.Empty;

        Assert.Empty(emptyArray);
        Assert.Equal(0, emptyArray.Length);
    }

    [Fact]
    public void Equals_WithSameArray_ReturnsTrue()
    {
        var array = new[] { 1, 2, 3 };
        var equatableArray1 = new EquatableArray<int>(array);
        var equatableArray2 = new EquatableArray<int>(array);

        Assert.True(equatableArray1.Equals(equatableArray2));
    }

    [Fact]
    public void Equals_WithDifferentArray_ReturnsFalse()
    {
        var equatableArray1 = new EquatableArray<int>([1, 2, 3]);
        var equatableArray2 = new EquatableArray<int>([4, 5, 6]);

        Assert.False(equatableArray1.Equals(equatableArray2));
    }

    [Fact]
    public void Equals_WithNull_ReturnsFalse()
    {
        var equatableArray = new EquatableArray<int>([1, 2, 3]);

        Assert.False(equatableArray.Equals(null));
    }

    [Fact]
    public void GetHashCode_WithSameArray_ReturnsSameHashCode()
    {
        var array = new[] { 1, 2, 3 };
        var equatableArray1 = new EquatableArray<int>(array);
        var equatableArray2 = new EquatableArray<int>(array);

        Assert.Equal(equatableArray1.GetHashCode(), equatableArray2.GetHashCode());
    }

    [Fact]
    public void GetEnumerator_ReturnsEnumerator()
    {
        var array = new[] { 1, 2, 3 };
        var equatableArray = new EquatableArray<int>(array);

        var enumerator = equatableArray.GetEnumerator();
        var list = new List<int>();

        while (enumerator.MoveNext())
        {
            list.Add(enumerator.Current);
        }

        Assert.Equal(array, list.ToArray());
    }

    [Fact]
    public void IEnumerable_GetEnumerator_ReturnsEnumerator()
    {
        var array = new[] { 1, 2, 3 };
        var equatableArray = new EquatableArray<int>(array);

        var enumerator = ((System.Collections.IEnumerable)equatableArray).GetEnumerator();
        var list = new List<int>();

        while (enumerator.MoveNext())
        {
            list.Add((int)enumerator.Current);
        }

        Assert.Equal(array, list.ToArray());
    }
}
