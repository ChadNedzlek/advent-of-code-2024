using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ChadNedzlek.AdventOfCode.Library;

namespace ChadNedzlek.AdventOfCode.Y2024.CSharp;

public class Problem21 : SyncProblemBase
{
    private static readonly Dictionary<char, GPoint2<int>> s_moves = new (){
        ['A'] = (0, 0),
        ['v'] = (1, 0),
        ['^'] = (-1, 0),
        ['<'] = (0, -1),
        ['>'] = (0, 1),
    };

    private static readonly Dictionary<GPoint2<int>, char> s_rev = s_moves.ToDictionary(s => s.Value, s => s.Key);
    
    protected override void ExecuteCore(string[] data)
    {
        char[,] primaryKeypad =
        {
            {'7','8','9'},
            {'4','5','6'},
            {'1','2','3'},
            {'\0','0','A'},
        };
        
        char[,] secondaryKeypad =
        {
            {'\0','^','A'},
            {'<','v','>'},
        };

        Dictionary<(char from, char to), ImmutableList<string>> primaryPaths  = FillPaths(primaryKeypad);
        Dictionary<(char from, char to), ImmutableList<string>> secondaryPaths  = FillPaths(secondaryKeypad);
        Dictionary<long, Dictionary<(char from, char to), long>> levelCosts = [];

        long iterations = 25;
        for (int i = 0; i < iterations; i++)
        {
            Dictionary<(char from, char to), long> distances = [];
            if (i == 0)
            {
                foreach (((char from, char to) key, ImmutableList<string> paths) in secondaryPaths)
                {
                    distances.Add(key, paths.Min(p => p.Length));
                }
            }
            else
            {
                var prev = levelCosts[i - 1];
                foreach (((char from, char to) key, ImmutableList<string> paths) in secondaryPaths)
                {
                    long best = long.MaxValue;
                    foreach (var path in paths)
                    {
                        best = long.Min(best, ("A" + path).Zip(path).Sum(d => prev[(d.First, d.Second)]));
                    }
                    distances.Add(key, best);
                }
            }
            levelCosts.Add(i, distances);
        }

        Dictionary<(char from, char to), long> fullIndirectionCost = [];
        {
            var prev = levelCosts[iterations - 1];
            foreach (((char from, char to) key, ImmutableList<string> paths) in primaryPaths)
            {
                long best = long.MaxValue;
                foreach (var path in paths)
                {
                    best = long.Min(best, ("A" + path).Zip(path).Sum(d => prev[(d.First, d.Second)]));
                }

                fullIndirectionCost.Add(key, best);
            }
        }
        Dictionary<(char from, char to), long> oneIndirectionCost = [];
        {
            var prev = levelCosts[1];
            foreach (((char from, char to) key, ImmutableList<string> paths) in primaryPaths)
            {
                long best = long.MaxValue;
                foreach (var path in paths)
                {
                    best = long.Min(best, ("A" + path).Zip(path).Sum(d => prev[(d.First, d.Second)]));
                }

                oneIndirectionCost.Add(key, best);
            }
        }

        long total1 = 0;
        long total2 = 0;
        foreach (var line in data.Where(l => !string.IsNullOrWhiteSpace(l)))
        {
            {
                var cost = ("A" + line).Zip(line).Sum(d => oneIndirectionCost[(d.First, d.Second)]);
                var value = long.Parse(line[..^1]);
                var part = cost * value;
                total1 += part;
                Helpers.VerboseLine($"Part 1: Line {line} has value {value} * {cost} = {part}");
            }
            {
                var cost = ("A" + line).Zip(line).Sum(d => fullIndirectionCost[(d.First, d.Second)]);
                var value = long.Parse(line[..^1]);
                var part = cost * value;
                total2 += part;
                Helpers.VerboseLine($"Part 2: Line {line} has value {value} * {cost} = {part}");
            }
        }
        Console.WriteLine($"Part 1: {total1}");
        Console.WriteLine($"Part 2: {total2}");

        Dictionary<(char from, char to),ImmutableList<string>> FillPaths(char[,] keypad)
        {
            Dictionary<(char from, char to), ImmutableList<string>> paths = [];
            foreach (var a in keypad.AsEnumerableWithPoint())
            {
                if (a.value == '\0') continue;
                foreach (var b in keypad.AsEnumerableWithPoint())
                {
                    if (b.value == '\0') continue;
                    paths.Add((a.value, b.value), GetPaths(a.point, b.point, keypad));
                }
            }

            return paths;

            ImmutableList<string> GetPaths(GPoint2<int> a, GPoint2<int> b, char[,] map)
            {
                if (a == b) return ["A"];
                var p = ImmutableList.CreateBuilder<string>();
                var distance = (b - a).OrthogonalDistance;
                for (var i = 0; i < Helpers.OrthogonalDirections.Length; i++)
                {
                    GPoint2<int> dir = Helpers.OrthogonalDirections[i];
                    GPoint2<int> nextPoint = a + dir;
                    if ((nextPoint - b).OrthogonalDistance >= distance) continue;
                    if (map.Get(nextPoint) == '\0') continue;

                    char d = s_rev[dir];
                    p.AddRange(GetPaths(nextPoint, b, map).Select(c => d + c));
                }

                return p.ToImmutable();
            }
        }
    }
}