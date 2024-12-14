using System.Numerics;

namespace ChadNedzlek.AdventOfCode.Library;

public readonly record struct Rect2<T>(Point2<T> BottomLeft, Point2<T> TopRight)
    : IMultiplyOperators<Rect2<T>, T, Rect2<T>>,
        IEqualityOperators<Rect2<T>, Rect2<T>, bool>
    where T : struct, IAdditionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IAdditiveIdentity<T, T>, IMultiplicativeIdentity<T, T>,
    ISubtractionOperators<T, T, T>, IUnaryNegationOperators<T, T>, IComparisonOperators<T, T, bool>, IDivisionOperators<T, T, T>, IModulusOperators<T, T, T>
{
    public T Top => TopRight.Y;
    public T Right => TopRight.X;
    
    public T Left => BottomLeft.X;
    public T Bottom => BottomLeft.Y;

    public T Height => Top - Bottom;
    public T Width => Right - Left;

    public static Rect2<T> operator *(Rect2<T> rect, T scale) => new(rect.BottomLeft * scale, rect.TopRight * scale);
    public static Rect2<T> operator *(T scale, Rect2<T> rect) => rect * scale;

    public bool Contains(Point2<T> p)
    {
        return p.X >= Left && p.X <= Right && p.Y >= Bottom && p.Y <= Top;
    }
}