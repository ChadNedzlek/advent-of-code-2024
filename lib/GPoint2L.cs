using System.Numerics;

namespace ChadNedzlek.AdventOfCode.Library;

public readonly record struct GPoint2L(long Row, long Col) : IConvertable<(long r, long c), GPoint2L>
{
    public static implicit operator GPoint2L((long r, long c) p) => new(p.r, p.c);

    public GPoint2L Add(GPoint2L d)
    {
        return new GPoint2L(d.Row + Row, d.Col + Col);
    }

    public GPoint2L Add(long dRow, long dCol)
    {
        return new GPoint2L(dRow + Row, dCol + Col);
    }

    public bool Equals(long row, long col)
    {
        return Row == row && Col == col;
    }
}

public readonly record struct GPoint2I(int Row, int Col) : IConvertable<(int row, int col), GPoint2I>
{
    public static implicit operator GPoint2I((int row, int col) p) => new(p.row, p.col);

    public GPoint2I Add(GPoint2I d)
    {
        return new GPoint2I(d.Row + Row, d.Col + Col);
    }

    public GPoint2I Add(int dRow, int dCol)
    {
        return new GPoint2I(dRow + Row, dCol + Col);
    }

    public bool Equals(int row, int col)
    {
        return Row == row && Col == col;
    }
}

public readonly record struct Point2L(long X, long Y) : IConvertable<(long x, long y), Point2L>
{
    public static implicit operator Point2L((long x, long y) p) => new(p.x, p.y);

    public Point2L Add(Point2L d)
    {
        return new Point2L(d.X + X, d.Y + Y);
    }

    public Point2L Add(long dx, long dy)
    {
        return new Point2L(dx + X, dy + Y);
    }

    public bool Equals(long x, long y)
    {
        return X == x && Y == y;
    }
}

public readonly record struct Point2I(int X, int Y) : IConvertable<(int x, int y), Point2I>
{
    public static implicit operator Point2I((int x, int y) p) => new(p.x, p.y);

    public Point2I Add(Point2I d)
    {
        return new Point2I(d.X + X, d.Y + Y);
    }

    public Point2I Add(int dx, int dy)
    {
        return new Point2I(dx + X, dy + Y);
    }

    public bool Equals(int x, int y)
    {
        return X == x && Y == y;
    }
}
