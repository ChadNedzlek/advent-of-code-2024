using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO.Hashing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ChadNedzlek.AdventOfCode.Library;

namespace ChadNedzlek.AdventOfCode.Y2024.CSharp;

public class Problem19 : SyncProblemBase
{
    protected override void ExecuteCore(string[] data)
    {
        var patterns = data[0].Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => new ComparableReadOnlyMemory<char>(s.Trim().AsMemory())).ToImmutableList();
        var goals = data.Skip(2).Select(d => new ComparableReadOnlyMemory<char>(d.AsMemory())).ToArray();

        var sw = Stopwatch.StartNew();
        int count = 0;
        HashSet<ComparableReadOnlyMemory<char>> toSkip = [];
        foreach (var d in goals)
        {
            var solution = new Algorithms.BasicPriorityState<TowelState, ComparableReadOnlyMemory<char>>(new TowelState(patterns, d), default, d).Search();
            if (solution is not null)
                count++;
            else
            {
                toSkip.Add(d);
            }
        }
        
        Console.WriteLine($"[TIME {sw.Elapsed.TotalMilliseconds}ms] {count} possible patterns");
        long allCount = 0;
        var taskBasedMemoSolver = new TaskBasedMemoSolver<AllState, long>();
        foreach (var d in goals)
        {
            if (toSkip.Contains(d)) continue;
            var solution = taskBasedMemoSolver.Solve(new AllState(patterns, d));
            allCount += solution;
        }
        
        Console.WriteLine($"[TIME {sw.Elapsed.TotalMilliseconds}ms] {allCount} possible permutated patterns patterns");
    }

    public class AllState : ITaskMemoState<AllState, long>, IEquatable<AllState>
    {
        private readonly ImmutableList<ComparableReadOnlyMemory<char>> _pieces;
        private readonly ComparableReadOnlyMemory<char> _target;
        private readonly ComparableReadOnlyMemory<char> _current;
        private readonly ComparableReadOnlyMemory<char> _rem;

        public AllState(ImmutableList<ComparableReadOnlyMemory<char>> pieces, ComparableReadOnlyMemory<char> target, ComparableReadOnlyMemory<char> current = default)
        {
            _pieces = pieces;
            _target = target;
            _current = current;
            _rem = target[current.Length..];
        }

        public async Task<long> Solve(IAsyncSolver<AllState, long> solver)
        {
            if (_rem.Length == 0) return 1;
            long count = 0;
            foreach (var p in _pieces)
            {
                if (_rem.Span.StartsWith(p.Span))
                {
                    count += await solver.GetSolutionAsync(new AllState(_pieces, _target, _target[..(_current.Length + p.Length)]));
                }
            }

            return count;
        }

        public bool Equals(AllState other) => _rem.Span.SequenceEqual(other._rem.Span);

        public override int GetHashCode() => _rem.GetHashCode();
    }

    private class TowelState : Algorithms.IPrioritySearchable<ComparableReadOnlyMemory<char>>
    {
        private readonly ImmutableList<ComparableReadOnlyMemory<char>> _pieces;
        private readonly ComparableReadOnlyMemory<char> _target;

        public TowelState(ImmutableList<ComparableReadOnlyMemory<char>> pieces, ComparableReadOnlyMemory<char> target)
        {
            _pieces = pieces;
            _target = target;
        }

        public long GetCost(ComparableReadOnlyMemory<char> from, ComparableReadOnlyMemory<char> to) => to.Length - from.Length;

        public IEnumerable<ComparableReadOnlyMemory<char>> GetNextValuesFrom(ComparableReadOnlyMemory<char> from)
        {
            var segment = _target[from.Length..];
            foreach (var p in _pieces)
            {
                if (segment.Span.StartsWith(p.Span))
                {
                    ComparableReadOnlyMemory<char> biggerStart = _target[..(from.Length + p.Length)];
                    yield return biggerStart;
                }
            }
        }
    }
}

public readonly struct ComparableReadOnlyMemory<T> : IEquatable<ComparableReadOnlyMemory<T>>
    where T : struct
{
    public readonly ReadOnlyMemory<T> Memory;

    public ComparableReadOnlyMemory(ReadOnlyMemory<T> memory)
    {
        Memory = memory;
    }

    public ReadOnlySpan<T> Span => Memory.Span;
    public int Length => Memory.Length;

    public bool Equals(ComparableReadOnlyMemory<T> other) => Span.SequenceEqual(other.Span);

    public override bool Equals(object obj)
    {
        return obj is ComparableReadOnlyMemory<T> other && Equals(other);
    }

    public override int GetHashCode()
    {
        return unchecked((int)XxHash32.HashToUInt32(MemoryMarshal.AsBytes(Span)));
    }

    public ComparableReadOnlyMemory<T> Slice(int start) => new(Memory.Slice(start));
    public ComparableReadOnlyMemory<T> Slice(int start, int length) => new(Memory.Slice(start, length));

    public static bool operator ==(ComparableReadOnlyMemory<T> left, ComparableReadOnlyMemory<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ComparableReadOnlyMemory<T> left, ComparableReadOnlyMemory<T> right)
    {
        return !left.Equals(right);
    }

    public override string ToString() => Memory.ToString();

    public static implicit operator ComparableReadOnlyMemory<T>(ReadOnlyMemory<T> other) => new (other);
}