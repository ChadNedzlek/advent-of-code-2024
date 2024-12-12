using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using ChadNedzlek.AdventOfCode.Library;

namespace ChadNedzlek.AdventOfCode.Y2024.CSharp;

public class Problem11 : SyncProblemBase
{
    protected override void ExecuteCore(string[] data)
    {
        var stones = data[0].Split().Select(ulong.Parse).ToImmutableArray();
        TaskBasedMemoSolver<StoneState, long> solver = new TaskBasedMemoSolver<StoneState, long>();
        var part1 = solver.Solve(new(stones, 25));
        var part2 = solver.Solve(new(stones, 75));
        
        Console.WriteLine($"Part 1: {part1}");
        Console.WriteLine($"Part 2: {part2}");
    }

    public readonly record struct StoneState(ImmutableArray<ulong> Stones, int RemainingIterations) : ITaskMemoState<StoneState, long>
    {
        public async Task<long> Solve(IAsyncSolver<StoneState, long> solver)
        {
            if (RemainingIterations == 0)
                return Stones.Length;
            
            return Stones switch
            {
                [var s] => await SolveSingleStone(s, solver),
                [.. var r, var s] => await SolveSingleStone(s, solver) + await solver.GetSolutionAsync(this with { Stones = Stones[..^1] }),
            };
        }

        private async Task<long> SolveSingleStone(ulong stone, IAsyncSolver<StoneState, long> solver)
        {
            if (stone == 0) return await solver.GetSolutionAsync(new([1], RemainingIterations - 1));

            string s = stone.ToString();
            if (s.Length % 2 == 0)
            {
                return await solver.GetSolutionAsync(new([ulong.Parse(s[..(s.Length / 2)]), ulong.Parse(s[(s.Length / 2)..])], RemainingIterations - 1));
            }

            return await solver.GetSolutionAsync(new([stone * 2024], RemainingIterations - 1));
        }

        public bool Equals(StoneState? other)
        {
            if (other is not { } state) return false;
            return Stones.SequenceEqual(state.Stones) && RemainingIterations == state.RemainingIterations;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Stones[0], RemainingIterations);
        }
    }
}