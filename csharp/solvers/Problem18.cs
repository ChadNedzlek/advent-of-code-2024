using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ChadNedzlek.AdventOfCode.Library;
using ComputeSharp;

namespace ChadNedzlek.AdventOfCode.Y2024.CSharp;

public class Problem18 : SyncProblemBase
{
    protected override void ExecuteCore(string[] data)
    {
        Span<GPoint2<int>> b = data.As<int, int>(@"^(\d+),(\d+)$").Select(x => (GPoint2<int>)x).ToArray();
        int size = Program.ExecutionMode == ExecutionMode.Sample ? 7 : 71;
        int[,] map = new int[size, size];
        for (var i = 0; i < b.Length; i++)
        {
            GPoint2<int> p = b[i];
            map.TrySet(p, i + 1);
        }

        GPoint2<int> startPoint = (0,0);
        GPoint2<int> endPoint = (size-1, size-1);
        var solution = new TimedMapSearch(map, 1024, startPoint, endPoint).Search();
        
        Console.WriteLine($"Distance: {solution.GetScore()}");

        var (offset, length) = (1025..).GetOffsetAndLength(b.Length);

        int BinarySearch(int start, int end)
        {
            if (start > end)
            {
                return -start;
            }

            int mid = (start + end) / 2;
            var hasSolution = new TimedMapSearch(map, mid, startPoint, endPoint).Search() is not null;
            if (hasSolution)
            {
                Helpers.VerboseLine($"Has solution at {mid} [{start},{end}] => [{mid + 1},{end}]");
                return BinarySearch(mid + 1, end);
            }
            else
            {
                Helpers.VerboseLine($"No  solution at {mid} [{start},{end}] => [{start},{mid - 1}]");
                return BinarySearch(start, mid - 1);
            }
        }

        var sw = Stopwatch.StartNew();
        int res = BinarySearch(offset, offset + length);
        GPoint2<int> blocking = b[-res];
        Console.WriteLine($"[TIME] {sw.Elapsed.TotalMilliseconds}ms");
        Console.WriteLine($"First blocking byte: {blocking.Row},{blocking.Col}");
    }

    private class TimedMapSearch : Algorithms.BasicPriorityState<GPoint2<int>>
    {
        public readonly int[,] Map;
        public readonly int Time;
        
        public TimedMapSearch(int[,] map, int time, GPoint2<int> start, GPoint2<int> end) : base(start, end)
        {
            Map = map;
            Time = time;
        }

        public TimedMapSearch(TimedMapSearch from, GPoint2<int> current) : base(from, current)
        {
            Map = from.Map;
            Time = from.Time;
        }

        protected override TimedMapSearch With(GPoint2<int> current) => new(this, current);

        protected override IEnumerable<GPoint2<int>> GetNextValues()
        {
            foreach (GPoint2<int> t in Helpers.OrthogonalDirections.Select(d => Current + d))
            {
                int time = Map.Get(t, -1);
                if (time == 0 || time > Time)
                {
                    yield return t;
                }
            }
        }

        protected override long GetCostTo(GPoint2<int> target) => (target - Current).OrthogonalDistance;
    }
}