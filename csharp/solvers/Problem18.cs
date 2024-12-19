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
        var solution = new Algorithms.BasicPriorityState<TimedMapSearchable, GPoint2<int>>(new TimedMapSearchable(map, 1024), startPoint, endPoint).Search();
        
        Console.WriteLine($"Distance: {solution.GetScore()}");

        var (offset, length) = (1025..).GetOffsetAndLength(b.Length);

        int BinarySearch(int start, int end)
        {
            if (start > end)
            {
                return -start;
            }

            int mid = (start + end) / 2;
            var hasSolution = new Algorithms.BasicPriorityState<TimedMapSearchable, GPoint2<int>>(new TimedMapSearchable(map, mid), startPoint, endPoint).Search() is not null;
            if (hasSolution)
            {
                Helpers.VerboseLine($"Has solution at {mid} [{start},{end}] => [{mid + 1},{end}]");
                return BinarySearch(mid + 1, end);
            }

            Helpers.VerboseLine($"No  solution at {mid} [{start},{end}] => [{start},{mid - 1}]");
            return BinarySearch(start, mid - 1);
        }

        var sw = Stopwatch.StartNew();
        int res = BinarySearch(offset, offset + length);
        GPoint2<int> blocking = b[-res];
        Console.WriteLine($"[TIME] {sw.Elapsed.TotalMilliseconds}ms");
        Console.WriteLine($"First blocking byte: {blocking.Row},{blocking.Col}");
    }

    private class TimedMapSearchable : Algorithms.IPrioritySearchable<GPoint2<int>>
    {
        private readonly int[,] _map;
        private readonly int _time;
        
        public TimedMapSearchable(int[,] map, int time)
        {
            _map = map;
            _time = time;
        }

        public long GetCost(GPoint2<int> from, GPoint2<int> to) => (to - from).OrthogonalDistance;

        public IEnumerable<GPoint2<int>> GetNextValuesFrom(GPoint2<int> from)
        {
            foreach (GPoint2<int> t in Helpers.OrthogonalDirections.Select(d => from + d))
            {
                int time = _map.Get(t, -1);
                if (time == 0 || time > _time)
                {
                    yield return t;
                }
            }
        }
    }
}