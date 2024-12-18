using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ChadNedzlek.AdventOfCode.Library;

namespace ChadNedzlek.AdventOfCode.Y2024.CSharp;

public class Problem16 : SyncProblemBase
{
    protected override void ExecuteCore(string[] data)
    {
        string[] map = data;
        GPoint2<int> start = data.AsEnumerableWithPoint().FirstOrDefault(x => x.value == 'S').point;
        GPoint2<int> end = data.AsEnumerableWithPoint().FirstOrDefault(x => x.value == 'E').point;

        var result = Algorithms.PrioritySearch(
            new PriorityState(map, start, (0,1), end),
            s => s.Next(),
            s => s.Current == s.Target,
            s => (s.Target - s.Current).OrthogonalDistance + s.Cost,
            s => s.Current,
            s => s.Cost,
            (a, b) => a < b
        );

        var path = Backtrace(result);
        if (Helpers.IncludeVerboseOutput)
        {
            for (int r = 0; r < data.Length; r++)
            {
                for (int c = 0; c < data[r].Length; c++)
                {
                    var onPath = path.FirstOrDefault(s => s.Current == (r, c));
                    if (onPath != null)
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write(onPath.Direction switch
                        {
                            (1,0) => 'v',
                            (-1,0) => '^',
                            (0,1) => '>',
                            (0,-1) => '<',
                        });
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write(map[r][c]);
                    }
                }
                Console.WriteLine();
            }
            Console.ResetColor();
            Console.WriteLine();
        }

        Console.WriteLine($"Best cost?: {result.Cost}");

        Queue<(GPoint2<int> loc, GPoint2<int> dir, int cost)> evaluating = new();
        var costs = new[] { data.Select2D(_ => -1), data.Select2D(_ => -1), data.Select2D(_ => -1), data.Select2D(_ => -1) };
        costs[Helpers.OrthogonalDirections.IndexOf((0,1))].TrySet(start, 0);
        evaluating.Enqueue((start, (0,1), 0));

        while (evaluating.TryDequeue(out var search))
        {
            int prevCost = costs[Helpers.OrthogonalDirections.IndexOf(search.dir)].Get(search.loc);
            if (prevCost >= 0)
            {
                if (prevCost < search.cost)
                {
                    // Already been here cheaper
                    continue;
                }
            }
                
            costs[Helpers.OrthogonalDirections.IndexOf(search.dir)].TrySet(search.loc, search.cost);
            if (search.loc == end) continue;
            
            foreach (var dir in Helpers.OrthogonalDirections)
            {
                GPoint2<int> target = search.loc + dir;
                if (map.Get(target ,'#') == '#') continue;
                int newCost = search.cost + (dir != search.dir ? 1001 : 1);
                evaluating.Enqueue((target, dir, newCost));
            }
        }

        for (int i = 0; i < 4; i++)
        {
            costs[i].TrySet(end, result.Cost);
        }

        var onBestPath = data.Select2D(_ => false);
        onBestPath.TrySet(end, true);
        Queue<(GPoint2<int> location, ImmutableHashSet<int> costs)> pathQueue = new();
        pathQueue.Enqueue((end, [result.Cost]));
        while (pathQueue.TryDequeue(out var search))
        {
            if (search.location == start) continue;
            foreach (var dir in Helpers.OrthogonalDirections)
            {
                GPoint2<int> target = search.location+ dir;
                if (map.Get(target, '#') == '#') continue;
                var targetCosts = costs.Select(c => c.Get(target)).ToImmutableHashSet();
                var costIntersection = targetCosts.Intersect(search.costs.SelectMany(c => new[]{c - 1, c - 1001}));
                if (costIntersection.IsEmpty)
                {
                    continue;
                }

                if (onBestPath.Get(target))
                {
                    continue;
                }

                onBestPath.TrySet(target, true);
                pathQueue.Enqueue((target, costIntersection));
            }
        }
        
        if (Helpers.IncludeVerboseOutput)
        {
            for (int r = 0; r < data.Length; r++)
            {
                for (int c = 0; c < data[r].Length; c++)
                {
                    if (onBestPath.Get(r,c))
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("O");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write(map[r][c]);
                    }
                }
                Console.WriteLine();
            }
            Console.ResetColor();
            Console.WriteLine();
        }
        
        Console.WriteLine($"Things on best path: {onBestPath.AsEnumerable().Count(x => x)}");

        // var pathThings = bestPaths.SelectMany(p => Backtrace(p).Select(x => x.Current)).ToHashSet().Count;
        // Console.WriteLine($"Best path squares {pathThings}");
    }

    public readonly record struct AllPathResult(ImmutableHashSet<GPoint2<int>> PathPoints, int Cost)
    {
    }

    public record class AllPathState(string[] Map, GPoint2<int> Current, GPoint2<int> Direction, GPoint2<int> Target, int Cost = 0, ImmutableHashSet<GPoint2<int>> ExistingPoints = null) : ICallbackSolvable<AllPathState, AllPathResult>
    {
        public ISolution<AllPathState, AllPathResult> Solve()
        {
            if (Current == Target)
            {
                return new ImmediateSolution<AllPathState, AllPathResult>(new AllPathResult([Current], Cost));
            }

            var existing = ExistingPoints ?? [];

            var nextStates = Helpers.OrthogonalDirections.Where(d => Map.Get(Current + d, '#') != '#' && !existing.Contains(Current + d))
                .Select(
                    d => this with { Current = Current + d, Direction = d, Cost = Cost + 1 + (Direction == d ? 0 : 1000), ExistingPoints = existing.Add(Current)}
                )
                .ToImmutableList();
            return new DelegatedSolution<AllPathState, AllPathResult>(nextStates,
                nextResults =>
                {
                    if (nextResults.Count == 0)
                        return new AllPathResult([], Int32.MaxValue);
                    
                    var bestResults = nextResults.GroupBy(n => n.Cost).OrderBy(n => n.Key).First();
                    return new AllPathResult(bestResults.SelectMany(r => r.PathPoints).ToImmutableHashSet(), bestResults.Key);
                });
        }
    }

    private IEnumerable<PriorityState> Backtrace(PriorityState state)
    {
        while (state != null)
        {
            yield return state;
            state = state.Previous;
        }
    }

    private record PriorityState(string[] Map, GPoint2<int> Current, GPoint2<int> Direction, GPoint2<int> Target, int Cost = 0, PriorityState Previous = null) {
        public IEnumerable<PriorityState> Next()
        {
            return Helpers.OrthogonalDirections.Where(d => Map.Get(Current + d, '#') != '#' && d + Direction != (0, 0))
                .Select(
                    d => this with
                    {
                        Current = Current + d,
                        Direction = d,
                        Cost = Cost + 1 + (Direction == d ? 0 : 1000),
                        Previous = this,
                    }
                );
        }
    }
}