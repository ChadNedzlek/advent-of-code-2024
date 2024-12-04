using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace ChadNedzlek.AdventOfCode.Y2024.CSharp
{
    public class Problem02 : AsyncProblemBase
    {
        protected override async Task ExecuteCoreAsync(string[] data)
        {
            var part1 = data.Select(d => d.Split(' ').Select(int.Parse).ToImmutableList()).ToList();
            Console.WriteLine($"Safe: {part1.Count(IsSafe)}");
            Console.WriteLine($"Safish: {part1.Count(p => p.Select((_, i) => p.RemoveAt(i)).Any(IsSafe))}");
            return;

            static bool IsSafe(IReadOnlyList<int> list)
            {
                var differences = list.Zip(list.Skip(1)).Select(x => x.Second - x.First).ToList();
                var distance = differences.Select(Math.Abs).ToList();
                return distance.Min() >= 1 &&
                       distance.Max() <= 3 &&
                       differences.Select(Math.Sign).Distinct().Count() == 1;
            }
        }
    }
}