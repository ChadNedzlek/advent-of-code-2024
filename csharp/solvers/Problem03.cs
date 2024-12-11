using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ChadNedzlek.AdventOfCode.Library;

namespace ChadNedzlek.AdventOfCode.Y2024.CSharp;

public partial class Problem03 : AsyncProblemBase
{
    protected override async Task ExecuteCoreAsync(string[] data)
    {
        Console.WriteLine(
            "Result " + data.Aggregate(
                new State(),
                (s, line) => ParsingRegex.Matches(line)
                    .Aggregate(
                        s with { Do = true },
                        (s, m) => m.Groups[0].Value switch
                        {
                            "do()" => s with { Do = true },
                            "don't()" => s with { Do = false },
                            _ => (int.Parse(m.Groups[1].Value) * int.Parse(m.Groups[2].Value)).Into(s.Add),
                        }
                    )));
    }

    public readonly record struct State(bool Do = true, long Sum = 0, long ConditionalSum = 0)
    {
        public State Add(int value) => this with { Sum = Sum + value, ConditionalSum = ConditionalSum + (Do ? value : 0) };
    }

    [GeneratedRegex(@"mul\((\d{1,3}),(\d{1,3})\)|do\(\)|don't\(\)")]
    private static partial Regex ParsingRegex { get; }
}