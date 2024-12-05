using System;
using System.Linq;
using System.Threading.Tasks;
using ChadNedzlek.AdventOfCode.Library;

namespace ChadNedzlek.AdventOfCode.Y2024.CSharp
{
    public class Problem04 : AsyncProblemBase
    {
        protected override async Task ExecuteCoreAsync(string[] data)
        {
            int[] four = [0, 1, 2, 3];
            int[] three = [-1, 0, 1];

            Console.WriteLine(
                data.AsEnumerableWithIndex()
                    .Select((_, r, c) => new GPoint2<int>(r, c))
                    .Sum(
                        p => (
                            Helpers.EightDirections.Count(d => new string(four.Select(i => data.Get(p + d * i)).ToArray()) is "XMAS"),
                            Helpers.DiagonalDirections.Count(d => new string(three.Select(i => data.Get(p + d * i)).ToArray()) is "MAS") == 2 ? 1 : 0
                        )
                    )
            );
        }
    }
}