using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ChadNedzlek.AdventOfCode.DataModule;
using ChadNedzlek.AdventOfCode.Library;
using Mono.Options;
using Spectre.Console;

namespace ChadNedzlek.AdventOfCode.Y2024.CSharp;

public enum ExecutionMode
{
    Normal,
    Sample,
    Example = Sample,
    Test,
    TestOnly,
}

internal static class Program
{
    internal static ExecutionMode ExecutionMode { get; private set; }
        
    static async Task<int> Main(string[] args)
    {
        ExecutionMode = ExecutionMode.Normal;
        bool menu = false;
        int puzzle = 0;
        var os = new OptionSet
        {
            { "example|sample|s|e", v => ExecutionMode = ExecutionMode.Sample },
            { "test|t", v => ExecutionMode = ExecutionMode.Test },
            { "test-only", v => ExecutionMode = ExecutionMode.TestOnly },
            { "prompt|p", v => menu = v != null },
            { "verbose|v", v => Helpers.IncludeVerboseOutput = (v != null) },
            { "puzzle|z=", v => puzzle = int.Parse(v) },
        };

        var left = os.Parse(args);
        if (left.Count != 0)
        {
            Console.Error.WriteLine($"Unknown argument '{left[0]}'");
            return 1;
        }

        IProblemBase[] problems =
        [
            new Problem01(),
            new Problem02(),
            new Problem03(),
            new Problem04(),
            new Problem05(),
            new Problem06(),
            new Problem07(),
            new Problem08(),
            new Problem09(),
            new Problem10(),
            new Problem11(),
            new Problem12(),
            new Problem13(),
            new Problem14(),
            new Problem15(),
            new Problem16(),
            new Problem17(),
            new Problem18(),
            new Problem19(),
            new Problem20(),
            new Problem21(),
            new Problem22(),
            new Problem23(),
            new Problem24(),
            new Problem25(),
        ];

        if (puzzle != 0)
        {
            await problems[puzzle - 1].ExecuteAsync();
            return 0;
        }

        if (menu)
        {
            int problem = AnsiConsole.Prompt(new TextPrompt<int>("Which puzzle to execute?"));
            await problems[problem - 1].ExecuteAsync();
            return 0;
        }

        {
            foreach (IProblemBase problem in problems.Reverse())
            {
                try
                {
                    await problem.ExecuteAsync();
                    return 0;
                }
                catch (NotDoneException)
                {
                    // This puzzle isn't done yet, going back
                }
                catch (NoDataException)
                {
                    // This puzzle doesn't have data yet, going back
                }
            }
        }

        return 0;
    }
}