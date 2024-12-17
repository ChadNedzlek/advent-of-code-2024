using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ChadNedzlek.AdventOfCode.Library;

namespace ChadNedzlek.AdventOfCode.Y2024.CSharp;

public class Problem11 : SyncProblemBase
{
    protected override void ExecuteCore(string[] data)
    {
        var stones = data[0].Split().Select(ulong.Parse).ToImmutableArray();
        Stopwatch s = Stopwatch.StartNew();
        TaskBasedMemoSolver<StoneState, long> solver = new TaskBasedMemoSolver<StoneState, long>();
        var part1 = solver.Solve(new(stones, 25));
        var part2 = solver.Solve(new(stones, 75));
        Console.WriteLine($"[TIME] Tasky time {s.Elapsed}");
        
        Console.WriteLine($"Part 1: {part1}");
        Console.WriteLine($"Part 2: {part2}");
        s.Restart();
        var callbacky = new CallbackMemoSolver<StoneState, long>();
        part1 = callbacky.Solve(new(stones, 25));
        part2 = callbacky.Solve(new(stones, 75));
        Console.WriteLine($"[TIME] Callbacky time {s.Elapsed}");
        
        Console.WriteLine($"Part 1: {part1}");
        Console.WriteLine($"Part 2: {part2}");
    }

    public readonly struct StoneState : ITaskMemoState<StoneState, long>, IEquatable<StoneState>, ICallbackSolvable<StoneState, long>
    {
        public StoneState(ImmutableArray<ulong> stones, int remainingIterations)
        {
            Stones = stones;
            RemainingIterations = remainingIterations;
        }

        public async Task<long> Solve(IAsyncSolver<StoneState, long> solver)
        {
            if (RemainingIterations == 0)
                return Stones.Length;
            
            return Stones switch
            {
                [var s] => await SolveSingleStoneAsync(s, solver),
                [.. var r, var s] => await SolveSingleStoneAsync(s, solver) + await solver.GetSolutionAsync(this with { Stones = Stones[..^1] }),
            };
        }

        private async Task<long> SolveSingleStoneAsync(ulong stone, IAsyncSolver<StoneState, long> solver)
        {
            if (stone == 0) return await solver.GetSolutionAsync(new([1], RemainingIterations - 1));

            string s = stone.ToString();
            if (s.Length % 2 == 0)
            {
                return await solver.GetSolutionAsync(new([ulong.Parse(s[..(s.Length / 2)]), ulong.Parse(s[(s.Length / 2)..])], RemainingIterations - 1));
            }

            return await solver.GetSolutionAsync(new([stone * 2024], RemainingIterations - 1));
        }

        public ISolution<StoneState, long> Solve()
        {
            if (RemainingIterations == 0)
                return this.Immediate(Stones.Length);
            
            return Stones switch
            {
                [var s] => SolveSingleStoneCallback(s),
                [.. var r, var s] => this.Delegate([this with {Stones = [s]}, this with {Stones = r}], s => s.Sum())
            };
        }

        private ISolution<StoneState, long> SolveSingleStoneCallback(ulong stone)
        {
            if (stone == 0) return this.Delegate(new([1], RemainingIterations - 1), s => s);

            string s = stone.ToString();
            if (s.Length % 2 == 0)
            {
                return this.Delegate(new([ulong.Parse(s[..(s.Length / 2)]), ulong.Parse(s[(s.Length / 2)..])], RemainingIterations - 1), s => s);
            }

            return this.Delegate(new([stone * 2024], RemainingIterations - 1), s => s);
        }

        public override bool Equals(object obj)
        {
            if (obj is not StoneState s) return false;
            return Equals(s);
        }

        public bool Equals(StoneState other)
        {
            return Stones.SequenceEqual(other.Stones) && RemainingIterations == other.RemainingIterations;
        }

        public override int GetHashCode()
        {
            int hashCode = HashCode.Combine(Stones[0], RemainingIterations);
            return hashCode;
        }

        public ImmutableArray<ulong> Stones { get; init; }
        public int RemainingIterations { get; init; }
    }
}