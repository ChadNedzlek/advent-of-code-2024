using System;
using System.Collections.Generic;
using System.Linq;
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
        GPoint2<int> start = data.AsEnumerableWithPoint().First(x => x.value == 'S').point;
        GPoint2<int> end = data.AsEnumerableWithPoint().First(x => x.value == 'E').point;

        int[,] fromStartDistance = data.Select2D(_ => -1);
        int[,] fromEndDistance = data.Select2D(_ => -1);
        fromStartDistance.TrySet(start, 0);
        fromEndDistance.TrySet(end, 0);

        Queue<GPoint2<int>> search = [];
        search.Enqueue(start);
        while (search.TryDequeue(out var p))
        {
            var cost = fromStartDistance.Get(p);
            foreach (var dir in Helpers.OrthogonalDirections)
            {
                var target = p + dir;
                if (data.Get(target, '#') == '#') continue;

                int prevCost = fromStartDistance.Get(target);
                if (prevCost <= cost + 1 && prevCost >= 0) continue;

                fromStartDistance.TrySet(target, cost + 1);
                search.Enqueue(target);
            }
        }
        search.Enqueue(end);
        while (search.TryDequeue(out var p))
        {
            var cost = fromEndDistance.Get(p);
            foreach (var dir in Helpers.OrthogonalDirections)
            {
                var target = p + dir;
                if (data.Get(target, '#') == '#') continue;
                
                int prevCost = fromEndDistance.Get(target);
                if (prevCost <= cost + 1 && prevCost >= 0) continue;

                fromEndDistance.TrySet(target, cost + 1);
                search.Enqueue(target);
            }
        }

        int baseCost = fromStartDistance.Get(end);
        Dictionary<long, long> savingsAgg = [];
        int count = 0;
        foreach (var startCheat in data.AsEnumerableWithPoint().Where(x => x.value != '#').Select(x => x.point))
        foreach (var endCheat in data.AsEnumerableWithPoint().Where(x => x.value != '#').Select(x => x.point))
        {
            int cheatCost = (endCheat - startCheat).OrthogonalDistance;
            if (cheatCost > 20)
                continue;

            var before = fromStartDistance.Get(startCheat);
            var after = fromEndDistance.Get(endCheat);

            if (before == -1 || after == -1)
                continue;

            var savings = baseCost - (before + cheatCost + after);
            if (savings > 0)
            {
                savingsAgg.Increment(savings);
                if (savings >= 100)
                {
                    count++;
                }
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