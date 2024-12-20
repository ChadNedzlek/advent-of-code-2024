﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChadNedzlek.AdventOfCode.Library;

namespace ChadNedzlek.AdventOfCode.Y2024.CSharp;

public class Problem01 : AsyncProblemBase
{
    protected override async Task ExecuteCoreAsync(string[] data)
    {
        IEnumerable<(int, int)> part1 = data.As<int, int>(@"(\d+) *(\d+)").ToList();
        var left = part1.Select(x => x.Item1).OrderBy().ToList();
        var right = part1.Select(x => x.Item2).OrderBy().ToList();
        var sum = left.Zip(right).Sum(x => Math.Abs(x.First - x.Second));
        Console.WriteLine(sum);
        var part2 = left.Sum(l => right.Count(r => l == r) * l);
        Console.WriteLine(part2);
    }
}