using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace ChadNedzlek.AdventOfCode.Library;

public class CallbackMemoSolver<TState, TSolution> where TState : ICallbackSolvable<TState, TSolution>, IEquatable<TState>
{
    private static Dictionary<TState, TSolution> _solutions = new();

    private record struct PartialSolve(TState State, ISolution<TState, TSolution> Input, ImmutableList<TState> Required);

    public TSolution Solve(TState input)
    {
        if (_solutions.TryGetValue(input, out var solution))
        {
            // Caches solution
            return solution;
        }

        var inputSolution = input.Solve();
        if (inputSolution.TryGetSolution(out solution, out ImmutableList<TState> partials))
        {
            // Trivial case
            return solution;
        }

        Stack<PartialSolve> stack = new Stack<PartialSolve>();
        stack.Push(new PartialSolve(input, inputSolution, partials));
        while (stack.TryPop(out var s))
        {
            if (_solutions.ContainsKey(s.State))
                continue;

            List<TSolution> completedSolutions = new();
            HashSet<TState> missing = new();
            List<PartialSolve> partialSolutions = new();
            foreach (var p in s.Required)
            {
                if (_solutions.TryGetValue(p, out var pSol))
                {
                    completedSolutions.Add(pSol);
                }
                else
                {
                    var inprogress = p.Solve();
                    if (inprogress.TryGetSolution(out pSol, out var newRequired))
                    {
                        _solutions.Add(p, pSol);
                        completedSolutions.Add(pSol);
                    }
                    else if (missing.Add(p))
                    {
                        partialSolutions.Add(new PartialSolve(p, inprogress, newRequired));
                    }
                }
            }

            if (missing.Count == 0)
            {
                _solutions.Add(s.State, s.Input.GetSolution(completedSolutions));
            }
            else
            {
                // We aren't ready yet
                stack.Push(s);
                foreach (var p in partialSolutions)
                {
                    stack.Push(p);
                }
            }
        }

        return _solutions[input];
    }
}

public interface ICallbackSolvable<TState, TSolution>
    where TState : ICallbackSolvable<TState, TSolution>, IEquatable<TState>
{
    ISolution<TState, TSolution> Solve();
}

public abstract class CallbackSolvable<TState, TSolution>
    : ICallbackSolvable<TState, TSolution>
    where TState : ICallbackSolvable<TState, TSolution>, IEquatable<TState>
{
    public ISolution<TState, TSolution> Immediate(TSolution value) => new ImmediateSolution<TState, TSolution>(value);

    public static ISolution<TState, TSolution> Delegate(TState input) =>
        new DelegatedSolution<TState, TSolution>(ImmutableList.Create(input), l => l[0]);

    public static ISolution<TState, TSolution> Delegate(TState input, Func<TSolution, TSolution> transform) =>
        new DelegatedSolution<TState, TSolution>(ImmutableList.Create(input), l => transform(l[0]));

    public static ISolution<TState, TSolution> Delegate(TState a, TState b, Func<TSolution, TSolution, TSolution> transform) =>
        new DelegatedSolution<TState, TSolution>(ImmutableList.Create(a, b), l => transform(l[0], l[1]));

    public abstract ISolution<TState, TSolution> Solve();
}

public interface ISolution<TState, TSolution>
{
    bool TryGetSolution(out TSolution solution, out ImmutableList<TState> required);
    TSolution GetSolution(IReadOnlyList<TSolution> partials);
}

public class ImmediateSolution<TState, TSolution> : ISolution<TState, TSolution>
{
    private readonly TSolution _value;

    public ImmediateSolution(TSolution value)
    {
        _value = value;
    }

    public bool TryGetSolution(out TSolution solution, out ImmutableList<TState> required)
    {
        solution = _value;
        required = null;
        return true;
    }

    public TSolution GetSolution(IReadOnlyList<TSolution> partials)
    {
        return _value;
    }
}

public class DelegatedSolution<TState, TSolution> : ISolution<TState, TSolution>
{
    private readonly ImmutableList<TState> _inputs;
    private readonly Func<IReadOnlyList<TSolution>, TSolution> _combine;

    public DelegatedSolution(ImmutableList<TState> inputs, Func<IReadOnlyList<TSolution>, TSolution> combine)
    {
        _inputs = inputs;
        _combine = combine;
    }

    public bool TryGetSolution(out TSolution solution, out ImmutableList<TState> required)
    {
        solution = default;
        required = _inputs;
        return false;
    }

    public TSolution GetSolution(IReadOnlyList<TSolution> partials)
    {
        return _combine(partials);
    }
}