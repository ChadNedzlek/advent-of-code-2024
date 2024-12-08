using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ChadNedzlek.AdventOfCode.Library;

namespace ChadNedzlek.AdventOfCode.Y2024.CSharp
{
    public class Problem08 : DualProblemBase
    {
        protected override void ExecutePart1(string[] data)
        {
            int h = data.Length;
            int w = data[0].Length;
            var c = data.AsEnumerableWithIndex().Count(x => IsAntiNode(x.index0, x.index1));
            Console.WriteLine($"Found {c} antinodes");

            bool IsAntiNode(int r, int c) =>
                data.AsEnumerableWithIndex()
                    .Any(
                        x => char.IsLetterOrDigit(x.value) &&
                            (x.index0 != r || x.index1 != c) &&
                            data.Get((x.index0 - r) * 2 + r, (x.index1 - c) * 2 + c) == x.value
                    );
        }
        
        protected override void ExecutePart2(string[] data)
        {
            int h = data.Length;
            int w = data[0].Length;
            var c = data.AsEnumerableWithPoint().Count(IsAntiNode);
            Console.WriteLine($"Found {c} antinodes");

            bool IsAntiNode((char value, GPoint2<int> point) t) =>
                data.AsEnumerableWithPoint()
                    .Any(
                        x =>
                        {
                            if (!char.IsLetterOrDigit(x.value)) return false;
                            if (x == t) return false;

                            var diff = x.point - t.point;
                            int gcd;
                            if (diff.Row == 0) gcd = diff.Col;
                            else if (diff.Col == 0) gcd = diff.Row;
                            else gcd = Helpers.Gcd(Math.Abs(diff.Row), Math.Abs(diff.Col));
                            var step = diff / gcd;
                            var search = t.point;
                            while (data.IsInRange(search))
                            {
                                if (data.Get(search) == x.value && search != x.point)
                                {
                                    Helpers.VerboseLine($"Antinode at {t} for {x} at {search}");
                                    return true;
                                }

                                search += step;
                            }

                            return false;
                        }
                    );
        }

    }
}