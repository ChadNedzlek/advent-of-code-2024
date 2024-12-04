using System.Numerics;

namespace ChadNedzlek.AdventOfCode.Library;

public readonly record struct GPoint2<T>(T Row, T Col)
    : IAdditionOperators<GPoint2<T>, GPoint2<T>, GPoint2<T>>,
        IAdditiveIdentity<GPoint2<T>, GPoint2<T>>,
        IMultiplyOperators<GPoint2<T>, T, GPoint2<T>>,
        IMultiplicativeIdentity<GPoint2<T>, T>,
        IDivisionOperators<GPoint2<T>, T, GPoint2<T>>,
        IModulusOperators<GPoint2<T>, GRect2<T>, GPoint2<T>>,
        IUnaryNegationOperators<GPoint2<T>, GPoint2<T>>,
        ISubtractionOperators<GPoint2<T>, GPoint2<T>, GPoint2<T>>,
        IConvertable<(T row, T col), GPoint2<T>>,
        IEqualityOperators<GPoint2<T>,GPoint2<T>,bool>
    where T : struct, IAdditionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IAdditiveIdentity<T, T>, IMultiplicativeIdentity<T, T>,
    ISubtractionOperators<T, T, T>, IUnaryNegationOperators<T, T>, IComparisonOperators<T, T, bool>, IDivisionOperators<T,T,T>
{
    public static GPoint2<T> operator +(GPoint2<T> left, GPoint2<T> right) => new(left.Row + right.Row, left.Col + right.Col);

    public static GPoint2<T> operator *(GPoint2<T> point, T scale) => new(point.Row * scale, point.Col * scale);

    public static GPoint2<T> operator *(T scale, GPoint2<T> point) => point * scale;

    public static implicit operator GPoint2<T>((T row, T col) p) => new(p.row, p.col);
    public static GPoint2<T> AdditiveIdentity => new(T.AdditiveIdentity, T.AdditiveIdentity);
    public static T MultiplicativeIdentity => T.MultiplicativeIdentity;

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

    public static GPoint2<T> operator -(GPoint2<T> value) => new(-value.Row, -value.Col);

    public static GPoint2<T> operator -(GPoint2<T> left, GPoint2<T> right) => new(left.Row - right.Row, left.Col - right.Col);
    public static GPoint2<T> operator /(GPoint2<T> point, T scale) => new(point.Row / scale, point.Col / scale);
}