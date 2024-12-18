using System;
using System.Linq;
using ChadNedzlek.AdventOfCode.Library;

namespace ChadNedzlek.AdventOfCode.Y2024.CSharp;

public class Problem18 : SyncProblemBase
{
    protected override void ExecuteCore(string[] data)
    {
        Span<GPoint2<int>> b = data.As<int, int>(@"^(\d+),(\d+)$").Select(x => (GPoint2<int>)x).ToArray();
        int size = Program.ExecutionMode == ExecutionMode.Sample ? 7 : 71;
        char[,] map = new char[size, size];
        foreach(var p in b[..1024])
        {
            map.TrySet(p, '#');
        }

        Algorithms.CharMapAStar search = new(map, (0,0), (size-1, size-1));
        var solution = search.Search();
        
        Console.WriteLine($"Distance: {solution.GetScore()}");

        foreach (var p in b[1024..])
        {
            map.TrySet(p, '#');
            solution = search.Search();
            if (solution == null)
            {
                Console.WriteLine($"First blocking byte: {p.Row},{p.Col}");
                break;
            }
        }
    }
}