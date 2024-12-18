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
        public async Task ExecuteAsync()
        {
            if (Program.ExecutionMode is ExecutionMode.Test or ExecutionMode.TestOnly)
            {
                await ExecuteTests();

                if (Program.ExecutionMode is ExecutionMode.TestOnly)
                    return;

            }
            
            var m = Regex.Match(GetType().Name, @"Problem(\d+)$");
            var id = int.Parse(m.Groups[1].Value);
            var data = await Data.GetDataAsync(id, Program.ExecutionMode != ExecutionMode.Normal);

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
        protected override async Task ExecuteCoreAsync(string[] data)
        {
            var s = Stopwatch.StartNew();
            try
            {
                ExecutePart2(data);
            }
            catch (NotDoneException)
            {
                ExecutePart1(data);
            }
            Helpers.VerboseLine($"Elapsed: {s.Elapsed}");
        }

        protected abstract void ExecutePart1(string[] data);
        
        protected virtual void ExecutePart2(string[] data)
        {
            throw new NotDoneException();
        }
    }

    public interface IProblemBase
    {
        Task ExecuteAsync();
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
    public abstract class SyncProblemBase : IProblemBase
    {
        public async Task ExecuteAsync()
        {
            var m = Regex.Match(GetType().Name, @"Problem(\d+)$");
            var id = int.Parse(m.Groups[1].Value);
            var data = await Data.GetDataAsync(id, Program.ExecutionMode != ExecutionMode.Normal);
            if (this is IFancyProblem fancy)
                fancy.ExecuteFancy(data);
            else
                ExecuteCore(data);
        }

        protected virtual void ExecuteCore(string[] data) => throw new NotDoneException();
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

public class NotDoneException : Exception
{
}