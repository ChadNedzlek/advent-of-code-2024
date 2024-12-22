using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace ChadNedzlek.AdventOfCode.Library;

public readonly struct Point3<T> : IAdditionOperators<Point3<T>, Point3<T>, Point3<T>>,
    IAdditiveIdentity<Point3<T>, Point3<T>>,
    IMultiplyOperators<Point3<T>, T, Point3<T>>,
    IMultiplicativeIdentity<Point3<T>, T>,
    IUnaryNegationOperators<Point3<T>, Point3<T>>,
    IConvertable<(T x, T y, T z), Point3<T>>,
    IEqualityOperators<Point3<T>,Point3<T>,bool>,
    IEquatable<Point3<T>>,
    ISubtractionOperators<Point3<T>, Point3<T>, Point3<T>>
    where T : struct, INumber<T>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Point3(T x, T y, T z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point3<T> operator +(Point3<T> left, Point3<T> right) => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point3<T> operator *(Point3<T> point, T scale) => new(point.X * scale, point.Y * scale, point.Z * scale);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point3<T> operator *(T scale, Point3<T> point) => point * scale;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Point3<T>((T x, T y, T z) p) => new(p.x, p.y, p.z);
    
    public static Point3<T> AdditiveIdentity
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new(T.AdditiveIdentity, T.AdditiveIdentity, T.AdditiveIdentity);
    }

    public static T MultiplicativeIdentity
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => T.MultiplicativeIdentity;
    }

    public readonly T X, Y, Z;

    public T OrthogonalDistance
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => T.Abs(X) + T.Abs(Y) + T.Abs(Z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out T x, out T y, out T z) => (x, y, z) = (X, Y, Z);

    public bool Equals(Point3<T> other)
    {
        return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
    }

    public override bool Equals(object obj)
    {
        return obj is Point3<T> other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z);
    }

    public static bool operator ==(Point3<T> left, Point3<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Point3<T> left, Point3<T> right)
    {
        return !left.Equals(right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Point3<T> Abs() => (T.Abs(X), T.Abs(Y), T.Abs(Z));

    public override string ToString() => $"{{{X}, {Y}, {Z}}}";
    public static Point3<T> operator -(Point3<T> value) => (-value.X, -value.Y, -value.Z);
    public static Point3<T> operator -(Point3<T> left, Point3<T> right) => left + -right;
}

public static class Point3
{
    public static Point3<long> ToLong(this Point3<int> p) => (p.X, p.Y, p.Z);
}