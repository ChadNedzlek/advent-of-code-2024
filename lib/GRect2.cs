using System.Numerics;

namespace ChadNedzlek.AdventOfCode.Library;

public readonly record struct GRect2<T>(GPoint2<T> TopLeft, GPoint2<T> BottomRight)
    : IMultiplyOperators<GRect2<T>, T, GRect2<T>>,
        IEqualityOperators<GRect2<T>, GRect2<T>, bool>
    where T : struct, INumber<T>
{
    public T Top => TopLeft.Row;
    public T Left => TopLeft.Col;
    public T Bottom => BottomRight.Row;
    public T Right => BottomRight.Col;

    public T Height => Bottom - Top;
    public T Width => Right - Left;

    public static GRect2<T> operator *(GRect2<T> rect, T scale) => new(rect.TopLeft * scale, rect.BottomRight * scale);
    public static GRect2<T> operator *(T scale, GRect2<T> rect) => rect * scale;
}