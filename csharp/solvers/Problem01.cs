using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChadNedzlek.AdventOfCode.Library;

namespace ChadNedzlek.AdventOfCode.Y2023.CSharp.solvers
{
    public class Problem01 : AsyncProblemBase
    {
        protected override async Task ExecuteCoreAsync(string[] data)
        {
            IEnumerable<(int, int)> part1 = data.Select(d => Data.Parse<int, int>(d, @"(\d+) *(\d+)")).ToList();
            var left = part1.Select(x => x.Item1).OrderBy(x => x).ToList();
            var right = part1.Select(x => x.Item2).OrderBy(x => x).ToList();
            var sum = left.Zip(right).Sum(x => Math.Abs(x.First - x.Second));
            Console.WriteLine(sum);
            var part2 = left.Sum(l => right.Count(r => l == r) * l);
            Console.WriteLine(part2);
        }

        private static IEnumerable<string> TranslateLines(string[] list)
        {
            foreach (var line in list)
            {
                StringBuilder b = new();
                for (var i = 0; i < line.Length; i++)
                {
                    if (line[i..].StartsWith("one"))
                        b.Append(1);
                    if (line[i..].StartsWith("two"))
                        b.Append(2);
                    if (line[i..].StartsWith("three"))
                        b.Append(3);
                    if (line[i..].StartsWith("four"))
                        b.Append(4);
                    if (line[i..].StartsWith("five"))
                        b.Append(5);
                    if (line[i..].StartsWith("six"))
                        b.Append(6);
                    if (line[i..].StartsWith("seven"))
                        b.Append(7);
                    if (line[i..].StartsWith("eight"))
                        b.Append(8);
                    if (line[i..].StartsWith("nine"))
                        b.Append(9);
                    if (line[i..].StartsWith("zero"))
                        b.Append(0);
                    if (char.IsDigit(line[i]))
                        b.Append(line[i]);
                }

                yield return b.ToString();
            }
        }
    }
}