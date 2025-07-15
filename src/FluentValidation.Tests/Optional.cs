namespace FluentValidation.Tests;

using System;
using System.Collections.Generic;

public readonly struct Optional<T> : IEquatable<Optional<T>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Optional{T}"/> struct.
    /// </summary>
    /// <param name="value">The value.</param>
    public Optional(T value)
    {
        Value = value;
        HasValue = true;
    }

    /// <summary>
    /// Gets a value indicating whether the optional value has a value.
    /// </summary>
    public bool HasValue { get; }

    /// <summary>
    /// Gets the value.
    /// </summary>
    public T Value { get; }

    /// <summary>
    /// Implicitly converts a value to an optional value.
    /// </summary>
    /// <param name="value">The value.</param>
    public static implicit operator Optional<T>(T value) => new(value);

    /// <summary>
    /// Implicitly converts an optional value to a value.
    /// </summary>
    /// <param name="optional">The optional value.</param>
    public static implicit operator T(Optional<T> optional) => optional.Value;

    /// <inheritdoc />
    public readonly bool Equals(Optional<T> other)
    {
        return HasValue == other.HasValue && EqualityComparer<T>.Default.Equals(Value, other.Value);
    }

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
        return obj is Optional<T> other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(HasValue, Value);
    }

    /// <summary>
    /// Determines whether two optional values are equal.
    /// </summary>
    /// <param name="left">Left optional value.</param>
    /// <param name="right">Right optional value.</param>
    /// <returns>True if the optional values are equal; otherwise, false.</returns>
    public static bool operator ==(Optional<T> left, Optional<T> right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two optional values are not equal.
    /// </summary>
    /// <param name="left">Left optional value.</param>
    /// <param name="right">Right optional value.</param>
    /// <returns>True if the optional values are not equal; otherwise, false.</returns>
    public static bool operator !=(Optional<T> left, Optional<T> right)
    {
        return !left.Equals(right);
    }
}
