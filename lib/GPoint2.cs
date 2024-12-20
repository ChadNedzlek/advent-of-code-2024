using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;

namespace ChadNedzlek.AdventOfCode.Library;

public readonly struct GPoint2<T> : IAdditionOperators<GPoint2<T>, GPoint2<T>, GPoint2<T>>,
        IAdditiveIdentity<GPoint2<T>, GPoint2<T>>,
        IMultiplyOperators<GPoint2<T>, T, GPoint2<T>>,
        IMultiplicativeIdentity<GPoint2<T>, T>,
        IDivisionOperators<GPoint2<T>, T, GPoint2<T>>,
        IModulusOperators<GPoint2<T>, GRect2<T>, GPoint2<T>>,
        IUnaryNegationOperators<GPoint2<T>, GPoint2<T>>,
        ISubtractionOperators<GPoint2<T>, GPoint2<T>, GPoint2<T>>,
        IConvertable<(T row, T col), GPoint2<T>>,
        IEqualityOperators<GPoint2<T>,GPoint2<T>,bool>,
        IEquatable<GPoint2<T>>
    where T : struct, INumber<T>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GPoint2(T row, T col)
    {
        Row = row;
        Col = col;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GPoint2<T> operator +(GPoint2<T> left, GPoint2<T> right) => new(left.Row + right.Row, left.Col + right.Col);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GPoint2<T> operator *(GPoint2<T> point, T scale) => new(point.Row * scale, point.Col * scale);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GPoint2<T> operator *(T scale, GPoint2<T> point) => point * scale;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator GPoint2<T>((T row, T col) p) => new(p.row, p.col);
    
    public static GPoint2<T> AdditiveIdentity
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new(T.AdditiveIdentity, T.AdditiveIdentity);
    }

    public static T MultiplicativeIdentity
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => T.MultiplicativeIdentity;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GPoint2<T> operator -(GPoint2<T> value) => new(-value.Row, -value.Col);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GPoint2<T> operator -(GPoint2<T> left, GPoint2<T> right) => new(left.Row - right.Row, left.Col - right.Col);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GPoint2<T> operator /(GPoint2<T> point, T scale) => new(point.Row / scale, point.Col / scale);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GPoint2<T> operator %(GPoint2<T> point, GRect2<T> bounds)
    {
        T row = point.Row;
        while (row < bounds.Top) row += bounds.Height;
        while (row >= bounds.Bottom) row -= bounds.Height;

        T col = point.Col;
        while (col < bounds.Left) col += bounds.Width;
        while (col >= bounds.Right) col -= bounds.Width;

        return new(row, col);
    }

    public readonly T Row;
    public readonly T Col;
    
    public T OrthogonalDistance
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => T.Abs(Row) + T.Abs(Col);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out T row, out T col)
    {
        row = Row;
        col = Col;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(GPoint2<T> other)
    {
        return Row == other.Row && Col == other.Col;
    }

    public override bool Equals(object obj)
    {
        return obj is GPoint2<T> other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Row, Col);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(GPoint2<T> left, GPoint2<T> right)
    {
        return left.Equals(right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(GPoint2<T> left, GPoint2<T> right)
    {
        return !left.Equals(right);
    }

    public override string ToString() => $"{{{Row}, {Col}}}";
}