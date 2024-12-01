using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace ChadNedzlek.AdventOfCode.Library;

public interface IAsyncSolver<TState, TSolution>
{
    Task<TSolution> GetSolutionAsync(TState state);
}

public class TaskBasedMemoSolver<TState, TSolution> : IAsyncSolver<TState, TSolution> where TState : ITaskMemoState<TState, TSolution>, IEquatable<TState>
{
    private readonly Dictionary<TState, TSolution> _solveCache = new();
    private readonly HashSet<TState> _started = new();
    private readonly CustomTaskFactory _custom = new();

    public TaskBasedMemoSolver(bool preCheckCycles = false)
    {
        PreCheckCycles = preCheckCycles;
    }

    public bool PreCheckCycles { get; }

    Task<TSolution> IAsyncSolver<TState, TSolution>.GetSolutionAsync(TState state) => GetSolutionAsync(state);

    private Task<TSolution> GetSolutionAsync(TState state)
    {
        if (_solveCache.TryGetValue(state, out TSolution sol))
        {
            return Task.FromResult(sol);
        }

        if (!_started.Add(state))
        {
            return Task.FromResult(default(TSolution));
        }

        return _custom.StartNew(() => SolveAsync(state)).Unwrap();
        
        async Task<TSolution> SolveAsync(TState s)
        {
            await Task.Yield();
            var result = await s.Solve(this);
            _solveCache.Add(s, result);
            return result;
        }
    }

    public TSolution Solve(TState state)
    {
        _ = GetSolutionAsync(state);
        _custom.WaitForAllTasks();
        return _solveCache[state];
    }

    internal class CustomTaskFactory : TaskFactory
    {
        private class WaitForExecuteScheduler : TaskScheduler
        {
            private readonly Stack<Task> _pending = new();

            protected override IEnumerable<Task> GetScheduledTasks()
            {
                return ImmutableList<Task>.Empty;
            }

            protected override void QueueTask(Task task)
            {
                _pending.Push(task);
            }

            protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
            {
                return !taskWasPreviouslyQueued && TryExecuteTask(task);
            }

            protected override bool TryDequeue(Task task)
            {
                bool tryDequeue = base.TryDequeue(task);
                return tryDequeue;
            }

            public void WaitForAllTasks()
            {
                while (_pending.TryPop(out var task))
                {
                    TryExecuteTask(task);
                }
            }
        }

        public CustomTaskFactory() : base(new WaitForExecuteScheduler())
        {
        }

        public void WaitForAllTasks()
        {
            ((WaitForExecuteScheduler)Scheduler).WaitForAllTasks();
        }
    }
}

public interface ITaskMemoState<TState, TSolution> where TState : ITaskMemoState<TState, TSolution>, IEquatable<TState> 
{
    Task<TSolution> Solve(IAsyncSolver<TState, TSolution> solver);
}
