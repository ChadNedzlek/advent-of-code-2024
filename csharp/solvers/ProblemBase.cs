using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ChadNedzlek.AdventOfCode.Library;
using JetBrains.Annotations;

namespace ChadNedzlek.AdventOfCode.Y2024.CSharp
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
    public abstract class AsyncProblemBase : IProblemBase
    {
        protected AsyncProblemBase(string executionMode)
        {
            ExecutionMode = executionMode;
        }

        public string ExecutionMode { get; }
        public async Task ExecuteAsync()
        {
            string type = ExecutionMode;
            if (ExecutionMode.StartsWith("test"))
            {
                await ExecuteTests();

                if (ExecutionMode.EndsWith("exit"))
                    return;

                type = "example";
            }
            
            var m = Regex.Match(GetType().Name, @"Problem(\d+)$");
            var id = int.Parse(m.Groups[1].Value);
            var data = await Data.GetDataAsync(id, type);

            if (this is IFancyAsyncProblem fancy)
                await fancy.ExecuteFancyAsync(data);
            else 
                await ExecuteCoreAsync(data);
        }

        protected virtual Task ExecuteTests()
        {
            return Task.CompletedTask;
        }

        protected abstract Task ExecuteCoreAsync(string[] data);
    }
    
    [UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
    public class DualAsyncProblemBase : AsyncProblemBase
    {
        public DualAsyncProblemBase(string executionMode) : base(executionMode)
        {
        }

        protected override async Task ExecuteCoreAsync(string[] data)
        {
            var s = Stopwatch.StartNew();
            try
            {
                await ExecutePart2Async(data);
            }
            catch (HalfDoneException)
            {
                await ExecutePart1Async(data);
            }
            Helpers.VerboseLine($"Elapsed: {s.Elapsed}");
        }

        protected virtual Task ExecutePart1Async(string[] data)
        {
            return Task.CompletedTask;
        }
        
        protected virtual Task ExecutePart2Async(string[] data)
        {
            throw new HalfDoneException();
        }

        protected class HalfDoneException : Exception
        {
        }
    }
    
    [UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
    public abstract class DualProblemBase : AsyncProblemBase
    {
        protected DualProblemBase(string executionMode) : base(executionMode)
        {
        }

        protected override async Task ExecuteCoreAsync(string[] data)
        {
            var s = Stopwatch.StartNew();
            try
            {
                ExecutePart2(data);
            }
            catch (HalfDoneException)
            {
                ExecutePart1(data);
            }
            Helpers.VerboseLine($"Elapsed: {s.Elapsed}");
        }

        protected abstract void ExecutePart1(string[] data);
        
        protected virtual void ExecutePart2(string[] data)
        {
            throw new HalfDoneException();
        }

        protected class HalfDoneException : Exception
        {
        }
    }

    public interface IProblemBase
    {
        string ExecutionMode { get; }
        Task ExecuteAsync();
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
    public abstract class SyncProblemBase : IProblemBase
    {
        protected SyncProblemBase(string executionMode)
        {
            ExecutionMode = executionMode;
        }

        public string ExecutionMode { get; private set; }

        public async Task ExecuteAsync()
        {
            var m = Regex.Match(GetType().Name, @"Problem(\d+)$");
            var id = int.Parse(m.Groups[1].Value);
            var data = await Data.GetDataAsync(id, ExecutionMode);
            if (this is IFancyProblem fancy)
                fancy.ExecuteFancy(data);
            else
                ExecuteCore(data);
        }

        protected abstract void ExecuteCore(string[] data);
    }

    public interface IFancyAsyncProblem
    {
        Task ExecuteFancyAsync(string[] data);
    }
    
    public interface IFancyProblem
    {
        void ExecuteFancy(string[] data);
    }
}