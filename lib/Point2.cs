using System.Numerics;

namespace ChadNedzlek.AdventOfCode.Library;

public readonly record struct Point2<T>(T X, T Y)
    : IAdditionOperators<Point2<T>, Point2<T>, Point2<T>>,
        IAdditiveIdentity<Point2<T>, Point2<T>>,
        IMultiplyOperators<Point2<T>, T, Point2<T>>,
        IMultiplicativeIdentity<Point2<T>, T>,
        IDivisionOperators<Point2<T>, T, Point2<T>>,
        IModulusOperators<Point2<T>, Rect2<T>, Point2<T>>,
        IUnaryNegationOperators<Point2<T>, Point2<T>>,
        ISubtractionOperators<Point2<T>, Point2<T>, Point2<T>>,
        IConvertable<(T X, T Y), Point2<T>>,
        IEqualityOperators<Point2<T>,Point2<T>,bool>
    where T : struct, IAdditionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IAdditiveIdentity<T, T>, IMultiplicativeIdentity<T, T>,
    ISubtractionOperators<T, T, T>, IUnaryNegationOperators<T, T>, IComparisonOperators<T, T, bool>, IDivisionOperators<T,T,T>, IModulusOperators<T, T, T>
{
    public static Point2<T> operator +(Point2<T> left, Point2<T> right) => new(left.X + right.X, left.Y + right.Y);

    public static Point2<T> operator *(Point2<T> point, T scale) => new(point.X * scale, point.Y * scale);

    public static Point2<T> operator *(T scale, Point2<T> point) => point * scale;

    public static implicit operator Point2<T>((T X, T Y) p) => new(p.X, p.Y);
    public static Point2<T> AdditiveIdentity => new(T.AdditiveIdentity, T.AdditiveIdentity);
    public static T MultiplicativeIdentity => T.MultiplicativeIdentity;

    public static Point2<T> operator %(Point2<T> point, Rect2<T> bounds)
    {
        return new(
            (point.X - bounds.Left).PosMod(bounds.Width) + bounds.Left,
            (point.Y - bounds.Bottom).PosMod(bounds.Height) + bounds.Bottom
        );
    }

    public static Point2<T> operator -(Point2<T> value) => new(-value.X, -value.Y);

    public static Point2<T> operator -(Point2<T> left, Point2<T> right) => new(left.X - right.X, left.Y - right.Y);
    public static Point2<T> operator /(Point2<T> point, T scale) => new(point.X / scale, point.Y / scale);
}