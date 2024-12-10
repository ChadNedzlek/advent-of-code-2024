using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChadNedzlek.AdventOfCode.Library;

namespace ChadNedzlek.AdventOfCode.Y2024.CSharp
{
    public class Problem05 : AsyncProblemBase
    {
        public Problem05(string executionMode) : base(executionMode)
        {
        }

        protected override async Task ExecuteCoreAsync(string[] data)
        {
            List<(int before, int after)> ordering = [];
            bool reading = false;
            int good = 0;
            int bad = 0;
            foreach (string line in data)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    reading = true;
                    continue;
                }

                if (reading)
                {
                    int[] pages = line.Split(',').Select(int.Parse).ToArray();
                    int[] o = (int[])pages.Clone();
                    Helpers.PartialOrder(o, (a, b) => ordering.Contains((b, a)));
                    if (pages.SequenceEqual(o))
                    {
                        good += o[o.Length / 2];
                    }
                    else
                    {
                        bad += o[o.Length / 2];
                    }
                }
                else
                {
                    var l = line.Split('|').Select(int.Parse).ToArray();
                    ordering.Add((l[0], l[1]));
                }
            }


            Console.WriteLine($"Already good: {good}");
            Console.WriteLine($"Made good: {bad}");
        }
    }
}