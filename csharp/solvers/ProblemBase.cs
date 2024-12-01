using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ChadNedzlek.AdventOfCode.Library;
using JetBrains.Annotations;

namespace ChadNedzlek.AdventOfCode.Y2023.CSharp.solvers
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
    public abstract class AsyncProblemBase : IProblemBase
    {
        public async Task ExecuteAsync(string type = "real")
        {
            if (type.StartsWith("test"))
            {
                await ExecuteTests();

                if (type.EndsWith("exit"))
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

    public interface IProblemBase
    {
        Task ExecuteAsync(string type = "real");
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
    public abstract class SyncProblemBase : IProblemBase
    {
        public async Task ExecuteAsync(string type = "real")
        {
            var m = Regex.Match(GetType().Name, @"Problem(\d+)$");
            var id = int.Parse(m.Groups[1].Value);
            var data = await Data.GetDataAsync(id, type);
            if (this is IFancyProblem fancy)
                fancy.ExecuteFancy(data);
            else
                ExecuteCore(data);
        }

        protected abstract void ExecuteCore(IEnumerable<string> data);
    }

    public interface IFancyAsyncProblem
    {
        Task ExecuteFancyAsync(string[] data);
    }
    
    public interface IFancyProblem
    {
        void ExecuteFancy(IEnumerable<string> data);
    }
}