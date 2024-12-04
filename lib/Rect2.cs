using System.Numerics;

namespace ChadNedzlek.AdventOfCode.Library;

public readonly record struct Rect2<T>(Point2<T> TopLeft, Point2<T> BottomRight)
    : IMultiplyOperators<Rect2<T>, T, Rect2<T>>,
        IEqualityOperators<Rect2<T>, Rect2<T>, bool>
    where T : struct, IAdditionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IAdditiveIdentity<T, T>, IMultiplicativeIdentity<T, T>,
    ISubtractionOperators<T, T, T>, IUnaryNegationOperators<T, T>, IComparisonOperators<T, T, bool>, IDivisionOperators<T, T, T>, IModulusOperators<T, T, T>
{
    public T Top => TopLeft.Y;
    public T Left => TopLeft.X;
    public T Bottom => BottomRight.Y;
    public T Right => BottomRight.X;

    public T Height => Top - Bottom;
    public T Width => Right - Left;

    public static Rect2<T> operator *(Rect2<T> rect, T scale) => new(rect.TopLeft * scale, rect.BottomRight * scale);
    public static Rect2<T> operator *(T scale, Rect2<T> rect) => rect * scale;
}