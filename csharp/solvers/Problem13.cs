using System;
using System.Numerics;
using ChadNedzlek.AdventOfCode.Library;

namespace ChadNedzlek.AdventOfCode.Y2024.CSharp;

public class Problem13 : SyncProblemBase
{
    protected override void ExecuteCore(string[] data)
    {
        long tokens = 0;
        for (int i = 0; i < data.Length; i+=4)
        {
            if (string.IsNullOrWhiteSpace(data[i])) break;
            Point2<long> a = data[i].Parse<int, int>(@"^Button .: X\+(\d+), Y\+(\d+)$");
            Point2<long> b = data[i+1].Parse<int, int>(@"^Button .: X\+(\d+), Y\+(\d+)$");
            Point2<long> prize = data[i + 2].Parse<int, int>(@"^Prize: X=(\d+), Y=(\d+)$");

            tokens += SearchPrize(a, b, prize);
        }
        Console.WriteLine($"Min tokens {tokens}");
        
        long big = 0;
        for (int i = 0; i < data.Length; i+=4)
        {
            if (string.IsNullOrWhiteSpace(data[i])) break;
            Point2<long> a = data[i].Parse<int, int>(@"^Button .: X\+(\d+), Y\+(\d+)$");
            Point2<long> b = data[i+1].Parse<int, int>(@"^Button .: X\+(\d+), Y\+(\d+)$");
            Point2<long> prize = data[i + 2].Parse<int, int>(@"^Prize: X=(\d+), Y=(\d+)$");

            Point2<long> offset = (10000000000000, 10000000000000);
            big += SearchPrize(a, b, prize + offset);
        }
        Console.WriteLine($"Min tokens {big}");
    }

    private static long SearchPrize(Point2<long> a, Point2<long> b, Point2<long> prize)
    {
        // This is, in fact, a line intersection problem/linear algebra
        // Rearrange the equations:
        //   sa * a.x + sb * b.x = prize.x
        //   sa * a.y + sb * b.y = prize.y
        // In terms of first a, then b,
        // Then just check that they aren't negative (the lines intersect in the box)
        // and that they are integers (the lines intersect at a point)

        long denominator = a.Y * b.X - a.X * b.Y;
        
        (long sa, long rem) = long.DivRem(
            prize.Y * b.X - prize.X * b.Y,
            /*-------------------------*/
            denominator);
        if (rem != 0)
        {
            Helpers.VerboseLine($"Cannot reach prize at {prize} (a)");
            return 0;
        }

        (long sb, rem) = long.DivRem(
            prize.X * a.Y - prize.Y * a.X,
            /*-------------------------*/
            denominator
        );
        if (rem != 0)
        {
            Helpers.VerboseLine($"Cannot reach prize at {prize} (b)");
            return 0;
        }

        Helpers.VerboseLine($"Reached prize with {sa} A and {sb} B");
        return 3 * sa + sb;
    }
}