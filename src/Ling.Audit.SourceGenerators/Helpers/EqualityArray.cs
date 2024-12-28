using System.Collections;

namespace Ling.Audit.SourceGenerators.Helpers;

/// <summary>
/// Represents an array that can be compared for equality.
/// </summary>
/// <typeparam name="T">The type of elements in the array.</typeparam>
internal sealed class EquatableArray<T> : IEquatable<EquatableArray<T>>, IReadOnlyCollection<T>
{
    /// <summary>
    /// Gets an empty <see cref="EquatableArray{T}"/>.
    /// </summary>
    public static EquatableArray<T> Empty => new([]);

    private readonly T[] _array;

    /// <summary>
    /// Initializes a new instance of the <see cref="EquatableArray{T}"/> class with the specified array.
    /// </summary>
    /// <param name="array">The array to wrap.</param>
    public EquatableArray(T[] array)
    {
        _array = array;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EquatableArray{T}"/> class with the specified collection.
    /// </summary>
    /// <param name="collection">The collection to wrap.</param>
    public EquatableArray(IEnumerable<T> collection)
        : this(collection.ToArray())
    {
    }

    /// <summary>
    /// Gets the number of elements in the array.
    /// </summary>
    public int Count => _array.Length;

    /// <summary>
    /// Gets the length of the array.
    /// </summary>
    public int Length => _array.Length;

    /// <summary>
    /// Determines whether the specified <see cref="EquatableArray{T}"/> is equal to the current <see cref="EquatableArray{T}"/>.
    /// </summary>
    /// <param name="other">The <see cref="EquatableArray{T}"/> to compare with the current <see cref="EquatableArray{T}"/>.</param>
    /// <returns><c>true</c> if the specified <see cref="EquatableArray{T}"/> is equal to the current <see cref="EquatableArray{T}"/>; otherwise, <c>false</c>.</returns>
    public bool Equals(EquatableArray<T>? other)
    {
        return other is not null && (ReferenceEquals(this, other) || _array.SequenceEqual(other._array));
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is EquatableArray<T> other && Equals(other);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var item in _array)
        {
            hash.Add(item);
        }
        return hash.ToHashCode();
    }

    /// <summary>
    /// Returns an enumerator that iterates through the array.
    /// </summary>
    /// <returns>An enumerator for the array.</returns>
    public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)_array).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
