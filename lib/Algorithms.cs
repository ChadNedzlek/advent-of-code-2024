using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace ChadNedzlek.AdventOfCode.Library;

public static class Algorithms
{
    public class QueueReadySignal
    {
        private volatile TaskCompletionSource<bool> _source = new TaskCompletionSource<bool>();

        public Task<bool> WaitAsync() { return _source.Task; }

        public void Set(bool result)
        {
            if (_source.Task.IsCompleted)
                return;
            var old = Interlocked.Exchange(ref _source, new TaskCompletionSource<bool>());
            old.TrySetResult(result);
        }
    }

    public abstract class PriorityState<TState, TPriority, TIdentity, TScore>
        where TState : PriorityState<TState, TPriority, TIdentity, TScore>
    {
        public abstract IEnumerable<TState> GetNextState();
        public abstract bool IsEndState();
        public abstract long GetPriority();
        public abstract TIdentity GetIdentity();
        public abstract TScore GetScore();
        public abstract bool IsBetterScore(TScore a, TScore b);
        public abstract IComparer<TScore> ScoreComparer { get; }

        public TState Search() => PrioritySearchSingle<TState, TPriority, TIdentity, TScore>((TState)this);
        public List<TState> SearchAll() => PrioritySearchAll<TState, TPriority, TIdentity, TScore>((TState)this);
    }

    public class CharMapAStar : BasicPriorityState<GPoint2<int>>
    {
        public char[,] Map { get; }
        public bool AllowDiagonals { get; }
        public ImmutableHashSet<char> BlockingCharacters { get; }

        public CharMapAStar(char[,] map, GPoint2<int> start, GPoint2<int> end, bool allowDiagonals = false, params ImmutableHashSet<char> blockingCharacters) : base(start, end)
        {
            Map = map;
            AllowDiagonals = allowDiagonals;
            BlockingCharacters = blockingCharacters?.IsEmpty is not true ? blockingCharacters : ['#'];
        }

        public CharMapAStar(
            IReadOnlyList<string> map,
            GPoint2<int> start,
            GPoint2<int> end,
            bool allowDiagonals = false,
            params ImmutableHashSet<char> blockingCharacters
        ) : this(map.Select2D(c => c), start, end, allowDiagonals, blockingCharacters)
        {
        }

        private CharMapAStar(CharMapAStar from, GPoint2<int> current) : base(from, current)
        {
            Map = from.Map;
            BlockingCharacters = from.BlockingCharacters;
        }

        protected override BasicPriorityState<GPoint2<int>> With(GPoint2<int> current)
        {
            return new CharMapAStar(this, current);
        }

        protected override IEnumerable<GPoint2<int>> GetNextValues()
        {
            char defaultBlocking = BlockingCharacters.First();
            return (AllowDiagonals ? Helpers.EightDirections : Helpers.OrthogonalDirections).Select(d => d + Current).Where(t => !BlockingCharacters.Contains(Map.Get(t, defaultBlocking)));
        }

        protected override long GetCostTo(GPoint2<int> target) => (target - Current).OrthogonalDistance;
        public override IComparer<long> ScoreComparer => Comparer<long>.Default;
    }

    public abstract class BasicPriorityState<T> : PriorityState<BasicPriorityState<T>, long, T, long>
        where T : IEquatable<T>
    {
        public readonly T Current;
        public readonly T End;
        public readonly long Cost;

        public BasicPriorityState(T start, T end)
        {
            Current = start;
            End = end;
        }
        
        protected BasicPriorityState(BasicPriorityState<T> from, T current)
        {
            Current = current;
            End = from.End;
            Cost = from.Cost + from.GetEstimateTo(current);
        }

        public override IEnumerable<BasicPriorityState<T>> GetNextState()
        {
            return GetNextValues().Select(n => With(n));
        }

        protected abstract BasicPriorityState<T> With(T current);

        protected abstract IEnumerable<T> GetNextValues();

        protected virtual long GetEstimateTo(T target) => GetCostTo(target);

        protected abstract long GetCostTo(T target);

        public override bool IsEndState() => Current.Equals(End);

        public override long GetPriority() => Cost + GetEstimateTo(End);

        public override T GetIdentity() => Current;

        public override long GetScore() => Cost;

        public override bool IsBetterScore(long a, long b) => a.CompareTo(b) < 0;
    }
    
    public interface IPrioritySearchable<T>
    {
        long GetCost(T from, T to);
        IEnumerable<T> GetNextValuesFrom(T from);
        public long GetEstimate(T from, T to) => GetCost(from, to);
        public bool ReachedGoal(T test, T goal) => test.Equals(goal);
    }

    public class BasicPriorityState<TDriver, TSearch> : PriorityState<BasicPriorityState<TDriver, TSearch>, long, TSearch, long>
        where TDriver : IPrioritySearchable<TSearch>
        where TSearch : IEquatable<TSearch>
    {
        private readonly TDriver _driver;
        public readonly TSearch Current;
        public readonly TSearch End;
        public readonly long Cost;

        public BasicPriorityState(TDriver driver, TSearch start, TSearch end)
        {
            _driver = driver;
            Current = start;
            End = end;
        }

        private BasicPriorityState(BasicPriorityState<TDriver, TSearch> from, TSearch current)
        {
            _driver = from._driver;
            Current = current;
            End = from.End;
            Cost = from.Cost + _driver.GetCost(from.Current, current);
        }

        public override IEnumerable<BasicPriorityState<TDriver, TSearch>> GetNextState()
        {
            return _driver.GetNextValuesFrom(Current).Select(n => new BasicPriorityState<TDriver, TSearch>(this, n));
        }

        public override bool IsEndState() => _driver.ReachedGoal(Current, End);

        public override long GetPriority() => Cost + _driver.GetEstimate(Current, End);

        public override TSearch GetIdentity() => Current;

        public override long GetScore() => Cost;

        public override bool IsBetterScore(long a, long b) => a.CompareTo(b) < 0;
        public override IComparer<long> ScoreComparer => Comparer<long>.Default;
    }

    public static TState PrioritySearchSingle<TState, TPriority, TIdentity, TScore>(TState start)
        where TState : PriorityState<TState, TPriority, TIdentity, TScore>
        => PrioritySearchSingle(
            start,
            s => s.GetNextState(),
            s => s.IsEndState(),
            s => s.GetPriority(),
            s => s.GetIdentity(),
            s => s.GetScore(),
            start.ScoreComparer);
    public static List<TState> PrioritySearchAll<TState, TPriority, TIdentity, TScore>(TState start)
        where TState : PriorityState<TState, TPriority, TIdentity, TScore>
        => PrioritySearchAll(
            start,
            s => s.GetNextState(),
            s => s.IsEndState(),
            s => s.GetPriority(),
            s => s.GetIdentity(),
            s => s.GetScore(),
            start.ScoreComparer);

    public static TState PrioritySearchSingle<TState, TPriority, TIdentity, TScore>(
        TState initial,
        Func<TState, IEnumerable<TState>> nextStates,
        Func<TState, bool> isEndState,
        Func<TState, TPriority> getPriority,
        Func<TState, TIdentity> getIdentity,
        Func<TState, TScore> getScore,
        IComparer<TScore> scoreCompare
    )
        => PrioritySearchCore(
                initial,
                nextStates,
                isEndState,
                getPriority,
                getIdentity,
                getScore,
                scoreCompare,
                findAllBest: false
            )
            .singleState;
    
    public static List<TState> PrioritySearchAll<TState, TPriority, TIdentity, TScore>(
        TState initial,
        Func<TState, IEnumerable<TState>> nextStates,
        Func<TState, bool> isEndState,
        Func<TState, TPriority> getPriority,
        Func<TState, TIdentity> getIdentity,
        Func<TState, TScore> getScore,
        IComparer<TScore> scoreCompare
    )
        => PrioritySearchCore(
                initial,
                nextStates,
                isEndState,
                getPriority,
                getIdentity,
                getScore,
                scoreCompare,
                findAllBest: true
            )
            .allStates;

    private static (TState singleState, List<TState> allStates) PrioritySearchCore<TState, TPriority, TIdentity, TScore>(TState initial,
        Func<TState, IEnumerable<TState>> nextStates,
        Func<TState, bool> isEndState,
        Func<TState, TPriority> getPriority,
        Func<TState, TIdentity> getIdentity,
        Func<TState, TScore> getCost,
        IComparer<TScore> costComparer,
        bool findAllBest = false)
    {
        PriorityQueue<TState, TPriority> queue = new();
        Dictionary<TIdentity, TScore> loopbackDetection = new();
        queue.Enqueue(initial, getPriority(initial));
        TScore currentLowestCost = default;
        List<TState> lowestCostStates = [];
        bool hasSolution = false;
        while (queue.TryDequeue(out var state, out var p))
        {
            if (isEndState(state))
            {
                if (!findAllBest)
                {
                    return (state, null);
                }

                TScore score = getCost(state);
                if (hasSolution)
                {
                    switch (costComparer.Compare(score, currentLowestCost))
                    {
                        case <0:
                            lowestCostStates.Clear();
                            currentLowestCost = score;
                            goto case 0;
                        case 0:
                            lowestCostStates.Add(state);
                            break;
                        case >1:
                            break;
                    }
                    continue;
                }

                hasSolution = true;
                currentLowestCost = score;
                lowestCostStates.Add(state);
                continue;
            }

            var next = nextStates(state);

            foreach (var n in next)
            {                    
                var score = getCost(n);
                if (hasSolution && costComparer.Compare(score, currentLowestCost) > 0)
                {
                    // It's already too expensive, discard it
                    continue;
                }

                if (getIdentity != null)
                {
                    var stateId = getIdentity(n);
                    ref var loopbackEntry = ref CollectionsMarshal.GetValueRefOrAddDefault(
                        loopbackDetection,
                        stateId,
                        out bool exists
                    );
                    if (exists)
                    {
                        if (costComparer.Compare(score, loopbackEntry) >= 0)
                        {
                            // We already had one, and it was already as good as or better
                            continue;
                        }

                    }

                    loopbackEntry = score;
                }

                TPriority priority = getPriority(n);
                queue.Enqueue(n, priority);
            }
        }

        return (default, lowestCostStates);
    }
    
    public static TState BreadthFirstSearch<TState, TIdentity, TScore>(TState initial,
        Func<TState, IEnumerable<TState>> nextStates,
        Func<TState, TState, bool> isBetterState,
        Func<TState, TIdentity> getIdentity,
        Func<TState, TScore> getScore,
        Func<TScore, TScore, bool> isBetterScore)
    {
        Queue<TState> queue = new();
        Dictionary<TIdentity, TScore> loopbackDetection = new();
        TState best = initial;
        queue.Enqueue(initial);
        while (queue.TryDequeue(out var state))
        {
            if (isBetterState(state, best))
                best = state;

            var next = nextStates(state);

            foreach (var n in next)
            {
                if (getIdentity != null)
                {
                    var stateId = getIdentity(n);
                    ref var loopbackEntry = ref CollectionsMarshal.GetValueRefOrAddDefault(
                        loopbackDetection,
                        stateId,
                        out bool exists
                    );
                    var score = getScore(n);
                    if (exists)
                    {
                        if (!isBetterScore(score, loopbackEntry))
                        {
                            // We already had one, and it was already as good as or better
                            continue;
                        }

                    }

                    loopbackEntry = score;
                }

                queue.Enqueue(n);
            }
        }

        return best;
    }

    public class Blist<T> : IEnumerable<T>
    {
        private int _size;
        private List<T> _array;
        public int Count => _size;

        public Blist(int size)
        {
            _array = new List<T>(size);
        }

        public void Add(T item)
        {
            if (_size < _array.Count)
            {
                _array[_size++] = item;
                return;
            }

            _array.Add(item);
            _size = _array.Count;
        }

        public void Clear()
        {
            _size = 0;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _array.Take(_size).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public static async Task<TState> BreadthFirstSearchAsync<TState, TIdentity, TScore>(TState initial,
        Action<TState, Action<TState>> nextStates,
        Func<TState, TState, bool> isBetterState,
        Func<TState, TIdentity> getIdentity,
        Func<TState, TScore> getScore,
        Func<TScore, TScore, bool> isBetterScore,
        int batchSize = 2000)
    {
        Queue<TState> queue = new();
        bool done = false;
        Dictionary<TIdentity, TScore> loopbackDetection = new(1_000_000);
        QueueReadySignal needEvent = new ();
        
        queue.Enqueue(initial);
        int parallelism = Environment.ProcessorCount;
        int executing = parallelism;
        var subResults = await Task.WhenAll(Enumerable.Repeat(0, parallelism).Select(_ => Task.Run(Run)));
        return subResults.Aggregate((a,b) => isBetterState(a,b) ? a : b);

        async Task<TState> Run()
        {
            TState best = initial;
            List<TState> batch = new (batchSize);
            List<TState> next = new (batchSize);
            while (true)
            {
                batch.Clear();
                lock (queue)
                {
                    while (batch.Count < batchSize && queue.TryDequeue(out var s))
                    {
                        batch.Add(s);
                    }
                }

                if (batch.Count == 0)
                {
                    if (done)
                        return best;

                    // There is nothing to read for some reason, we need to mark ourselves as not executing
                    var cx = Interlocked.Decrement(ref executing);

                    // If we are the last person (because the counter is zero), that means all threads are waiting
                    if (cx == 0)
                    {
                        done = true;
                        needEvent.Set(false);
                        return best;
                    }

                    // We weren't the last, wait for either more data, or the channel to close
                    if (!await needEvent.WaitAsync())
                    {
                        // The channel was closed, time to go
                        return best;
                    }

                    // We are not waiting anymore, reenter the executing state
                    Interlocked.Increment(ref executing);
                }

                next.Clear();
                foreach (var state in batch)
                {
                    if (isBetterState(state, best))
                    {
                        best = state;
                    }

                    nextStates(state, next.Add);
                }
                batch.Clear();

                if (getIdentity != null)
                {
                    var idScore = next.Select(s => (state: s, id: getIdentity(s), score: getScore(s)));
                    lock (loopbackDetection)
                    {
                        void FilterStates()
                        {
                            foreach (var (state, stateId, score) in idScore)
                            {
                                ref var loopbackEntry = ref CollectionsMarshal.GetValueRefOrAddDefault(
                                    loopbackDetection,
                                    stateId,
                                    out bool exists
                                );
                                
                                if (exists)
                                {
                                    if (!isBetterScore(score, loopbackEntry))
                                    {
                                        // We already had one, and it was already as good as or better
                                        continue;
                                    }
                                }

                                loopbackEntry = score;
                                batch.Add(state);
                            }
                        }

                        FilterStates();
                        (next, batch) = (batch, next);
                    }
                }


                lock (queue)
                {
                    foreach (var n in next)
                    {
                        queue.Enqueue(n);
                    }
                }
            }
        }
    }
    
    public static TState BreadthFirstSearch<TState, TIdentity, TScore>(TState initial,
        Func<TState, IList<TState>> nextStates,
        IComparer<TState> stateComparer,
        Func<TState, TIdentity> getIdentity,
        Func<TState, TScore> getScore,
        IComparer<TScore> scoreComparer)
    {
        return BreadthFirstSearch(
            initial,
            nextStates,
            (a, b) => stateComparer.Compare(a, b) > 0,
            getIdentity,
            getScore,
            (a, b) => scoreComparer.Compare(a, b) > 0
        );
    }
    
    public static TState BreadthFirstSearch<TState, TIdentity, TScore>(TState initial,
        Func<TState, IList<TState>> nextStates,
        Func<TState, TIdentity> getIdentity,
        Func<TState, TScore> getScore)
        where TState : IComparable<TState>
        where TScore : IComparable<TScore>
    {
        return BreadthFirstSearch(
            initial,
            nextStates,
            Comparer<TState>.Default, 
            getIdentity,
            getScore,
            Comparer<TScore>.Default
        );
    }
    
    public static TState BreadthFirstSearch<TState>(
        TState initial,
        Func<TState, IList<TState>> nextStates,
        Func<TState, TState, bool> isBetterState)
    {
        return BreadthFirstSearch<TState, int, int>(initial, nextStates, isBetterState, null, null, null);
    }
   
    public static long[,] DistanceFill(string[] input, GPoint2<int> start,
        Func<GPoint2<int>, IEnumerable<GPoint2<int>>> movePoints) =>
        DistanceFill(input, start, p => movePoints(p).Select(m => (m, 1L)));

    public static long[,] DistanceFill(string[] input, GPoint2<int> start, Func<GPoint2<int>, IEnumerable<(GPoint2<int>, long)>> movePoints)
    {
        long[,] res = new long[input.Length,input[0].Length];
        for (var r = 0; r < res.GetLength(0); r++)
        for (var c = 0; c < res.GetLength(1); c++)
        {
            res[r, c] = -1;
        }

        res[start.Row, start.Col] = 0;

        Queue<(GPoint2<int> p, long d)> q = new();
        q.Enqueue((start, 0));
        while (q.TryDequeue(out (GPoint2<int> p, long d) x))
        {
            (GPoint2<int> p, long d) = x;
            foreach ((GPoint2<int> target, long cost) m in movePoints(p))
            {
                if (res[m.target.Row, m.target.Col] == -1)
                {
                    res[m.target.Row, m.target.Col] = d + m.cost;
                    q.Enqueue((m.target, d + m.cost));
                }
            }
        }
        return res;
    }

    /// <summary>
    /// Get both solutions to a quadratic equation in the form a*x^2 + b*x + c = 0
    /// </summary>
    /// <returns>Pair of solutions, with the first always the smaller of the two. Will return a pair of <see cref="double.NaN"/>
    /// if there is no solution</returns>
    public static (double, double) SolveQuadratic(double a, double b, double c)
    {
        var r = Math.Sqrt(b * b - 4 * a * c);
        var d = 2 * a;
        var s1 = (-b - r) / d;
        var s2 = (-b + r) / d;
        
        // If a is negative, these will be in the wrong order, and it's annoying for the caller to have to check, so swap em
        if (s1 > s2)
            (s1, s2) = (s2, s1);
        return (s1, s2);
    }

    public static IEnumerable<T[]> Permute<T>(IReadOnlyList<T> options, int count)
    {
        int optionsCount = options.Count;
        int len = count;
        int[] i = new int[len];
        while (true)
        {
            yield return i.Select(x => options[x]).ToArray();
            
            i[0]++;
            for (int j = 0; i[j] >= optionsCount && j < len - 1; j++)
            {
                i[j] = 0;
                i[j + 1]++;
            }
            if (i[len-1] == optionsCount)
                yield break;
        }
    }

    public static IEnumerable<char[]> Permute(string options, int count)
    {
        int optionsCount = options.Length;
        int len = count;
        int[] i = new int[len];
        while (true)
        {
            yield return i.Select(x => options[x]).ToArray();
            
            i[0]++;
            for (int j = 0; i[j] >= optionsCount && j < len - 1; j++)
            {
                i[j] = 0;
                i[j + 1]++;
            }
            if (i[len-1] == optionsCount)
                yield break;
        }
    }

    public static IEnumerable<ImmutableList<int>> Distribute(int total, int buckets)
    {
        if (buckets == 1)
        {
            yield return ImmutableList.Create(total);
            yield break;
        }

        int[] b = new int[buckets];
        b[0] = total;
        while (true)
        {
            yield return b.ToImmutableList();
            
            b[0]--;
            b[1]++;

            if (b[0] == -1)
            {
                if (buckets == 2)
                    yield break;
                
                b[2]++;
                for (int i = 2; i<buckets-1 && b[2..].Sum() > total; i++)
                {
                    b[i] = 0;
                    b[i + 1]++;
                }

                b[1] = 0;
                b[0] = 0;
                b[0] = total - b.Sum();
            }

            if (b[0] == 0)
            {
                yield break;
            }
        }
    }
}