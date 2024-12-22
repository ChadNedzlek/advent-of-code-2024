using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using ChadNedzlek.AdventOfCode.Library;
using Google.Protobuf.Collections;

namespace ChadNedzlek.AdventOfCode.Y2024.CSharp;

public class Problem20 : DualProblemBase
{
    protected override void ExecutePart1(string[] data)
    {
        GPoint2<int> start = data.AsEnumerableWithPoint().First(x => x.value == 'S').point;
        GPoint2<int> end = data.AsEnumerableWithPoint().First(x => x.value == 'E').point;
        var baseResult = new Algorithms.BasicPriorityState<CheatingRacer, GPoint2<int>>(new CheatingRacer(data, (-1,-1)), start, end).Search();
        int count = 0;
        Dictionary<long, long> savingsAgg = [];
        foreach ((char value, GPoint2<int> point) in data.AsEnumerableWithPoint())
        {
            if (value != '#') continue;
            Algorithms.BasicPriorityState<CheatingRacer, GPoint2<int>> result = new Algorithms.BasicPriorityState<CheatingRacer, GPoint2<int>>(new CheatingRacer(data, point), start, end).Search();
            long savings = baseResult.Cost - result.Cost;
            savingsAgg.Increment(savings);
            if (result.Cost <= baseResult.Cost - 100)
            {
                count++;
            }
        }

        if (Helpers.IncludeVerboseOutput)
        {
            foreach ((long amount, long c) in savingsAgg.OrderBy(s => s.Key))
            {
                Console.WriteLine($"There are {c} cheats that save {amount} picoseconds");
            }
        }

        Console.WriteLine($"There are {count} worthwhile cheats");
    }

    protected override void ExecutePart2(string[] data)
    {
        int h = data.Length;
        int w = data[0].Length;

        GPoint2<int> start = default;
        GPoint2<int> end = default;
        {
            Span<char> searchFor = ['S', 'E'];
            for (var r = 0; r < data.Length; r++)
            {
                ReadOnlySpan<char> s = data[r];
                var index = s.IndexOfAny(searchFor);
                if (index == -1) continue;
                if (s[index] == 'S')
                    start = (r, index);
                else end = (r, index);
            }
        }

        short[,] fromStartDistance = FillDistances(data, start, h, w);
        // short baseCost = fromStartDistance[end.Row, end.Col];
        //Dictionary<long, long> savingsAgg = [];
        int count = 0;
        for (int r1 = 1; r1 < h - 1; r1++)
        {
            int r2Lower = int.Max(1, r1 - 20);
            int r2Upper = int.Min(h - 1, r1 + 20);
            for (int r2 = r2Lower; r2 <= r2Upper; r2++)
            {
                int rDistance = int.Abs(r2 - r1);
                for (int c1 = 1; c1 < w - 1; c1++)
                {
                    int c2Lower = int.Max(1, c1 - (20 - rDistance));
                    int c2Upper = int.Min(w - 1, c1 + (20 - rDistance));
                    var before = fromStartDistance[r1, c1];
                    for (int c2 = c2Lower; c2 <= c2Upper; c2++)
                    {
                        int cheatCost = rDistance + int.Abs(c2 - c1);
                        if (cheatCost > 20)
                            continue;
                        var after = fromStartDistance[r2, c2];

                        // if (before == -1 || after == -1)
                        //     continue;

                        var savings = after - before + cheatCost;
                        // if (savings > 0)
                        // {
                        //     // if (Helpers.IncludeVerboseOutput)
                        //     // {
                        //     //     savingsAgg.Increment(savings);
                        //     // }
                        // }

                        if (savings >= 100)
                        {
                            count++;
                        }
                    }
                }
            }
        }

        // if (Helpers.IncludeVerboseOutput)
        // {
        //     foreach ((long amount, long c) in savingsAgg.OrderBy(s => s.Key))
        //     {
        //         Console.WriteLine($"There are {c} cheats that save {amount} picoseconds");
        //     }
        // }

        Console.WriteLine($"There are {count} worthwhile cheats");
    }

    private static short[,] FillDistances(string[] data, GPoint2<int> fromPoint, int nRows, int nCols)
    {
        short[,] distances = new short[nRows, nCols];
        distances.AsFlatSpan().Fill(-1);
        distances[fromPoint.Row, fromPoint.Col] = 0;
        Queue<GPoint2<int>> search = [];
        search.Enqueue(fromPoint);
        while (search.TryDequeue(out var p))
        {
            short cost = distances[p.Row, p.Col];
            foreach (var dir in Helpers.OrthogonalDirections)
            {
                var target = p + dir;
                if (data.Get(target, '#') == '#') continue;

                short prevCost = distances[target.Row, target.Col];
                if (prevCost >= 0 && prevCost <= cost + 1) continue;

                distances[target.Row, target.Col] = (short)(cost + 1);
                search.Enqueue(target);
            }
        }

        return distances;
    }

    public class CheatingRacer : Algorithms.IPrioritySearchable<GPoint2<int>>
    {
        private readonly IReadOnlyList<string> _map;
        private readonly GPoint2<int> _cheatAt;

        public CheatingRacer(IReadOnlyList<string> map, GPoint2<int> cheatAt)
        {
            _map = map;
            _cheatAt = cheatAt;
        }

        public long GetCost(GPoint2<int> from, GPoint2<int> to) => (from - to).OrthogonalDistance;

        public IEnumerable<GPoint2<int>> GetNextValuesFrom(GPoint2<int> from)
        {
            foreach (var dir in Helpers.OrthogonalDirections)
            {
                var target = from + dir;
                if (_map.Get(target, '#') == '#')
                {
                    if (target != _cheatAt)
                    {
                        continue;
                    }
                }

                yield return target;
            }

            // if (from == _cheatAt)
            // {
            //     foreach (var a in Helpers.OrthogonalDirections)
            //     foreach (var b in Helpers.OrthogonalDirections)
            //     {
            //         var target = from + a + b;
            //         if (_map.Get(target, '#') == '#') continue;
            //         yield return target;
            //     }
            // }
        }
    }
    public class DurationCheatingRacer : Algorithms.IPrioritySearchable<GPoint2<int>>
    {
        private readonly IReadOnlyList<string> _map;
        private readonly GPoint2<int> _startCheatingAt;
        private readonly GPoint2<int> _endCheatingAt;

        public DurationCheatingRacer(IReadOnlyList<string> map, GPoint2<int> startCheatingAt, GPoint2<int> endCheatingAt)
        {
            _map = map;
            _startCheatingAt = startCheatingAt;
            _endCheatingAt = endCheatingAt;
        }

        public long GetCost(GPoint2<int> from, GPoint2<int> to) => (from - to).OrthogonalDistance;

        public IEnumerable<GPoint2<int>> GetNextValuesFrom(GPoint2<int> from)
        {
            if (from == _startCheatingAt)
            {
                yield return _endCheatingAt;
                yield break;
            }

            foreach (var dir in Helpers.OrthogonalDirections)
            {
                var target = from + dir;
                if (_map.Get(target, '#') == '#') continue;

                yield return target;
            }
        }
    }
}