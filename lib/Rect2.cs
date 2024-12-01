using System.Numerics;

namespace ChadNedzlek.AdventOfCode.Library;

public readonly record struct Rect2(int Left, int Top, int Right, int Bottom)
{
    public bool IsInBounds(GPoint2L p)
    {
        return p.Row >= Left && p.Row <= Right && p.Col >= Top && p.Col <= Bottom;
    }
}

public readonly record struct Rect2L(long Left, long Top, long Right, long Bottom)
{
    public bool IsInBounds(GPoint2L p)
    {
        return p.Row >= Left && p.Row <= Right && p.Col >= Top && p.Col <= Bottom;
    }
}