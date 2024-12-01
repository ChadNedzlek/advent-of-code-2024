using System.Numerics;

namespace ChadNedzlek.AdventOfCode.Library;

public readonly record struct Point3I(int X, int Y, int Z) : IConvertable<(int x,int y, int z), Point3I>
{
    public static implicit operator Point3I((int x, int y, int z) p) => new(p.x, p.y, p.z);

    public Point3I Add(Point3I d)
    {
        return new Point3I(d.X + X, d.Y + Y, d.Z + Z);
    }
        
    public Point3I Add(int dx, int dy, int dz)
    {
        return new Point3I(dx + X, dy + Y, dz + Z);
    }
}

public readonly record struct Point3L(long X, long Y, long Z) : IConvertable<(long x,long y, long z), Point3L>
{
    public static implicit operator Point3L((long x, long y, long z) p) => new(p.x, p.y, p.z);

    public Point3L Add(Point3L d)
    {
        return new Point3L(d.X + X, d.Y + Y, d.Z + Z);
    }
        
    public Point3L Add(long dx, long dy, long dz)
    {
        return new Point3L(dx + X, dy + Y, dz + Z);
    }
}