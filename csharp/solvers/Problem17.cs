using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ChadNedzlek.AdventOfCode.Library;

namespace ChadNedzlek.AdventOfCode.Y2024.CSharp;

public class Problem17 : SyncProblemBase
{
    protected override void ExecuteCore(string[] data)
    {
        Dictionary<string, long> registers = data.TakeWhile(l => !string.IsNullOrEmpty(l))
            .As<string, long>(@"^Register (\w+): (-?\d+)$")
            .ToDictionary(x => x.Item1, x => x.Item2);
        long a = registers["A"];

        List<byte> aParts = [];
        var x = a;
        while (x > 0)
        {
            aParts.Add((byte)(x % 8));
            x >>= 3;
        }
        Console.WriteLine($"A register: {string.Join(',', aParts)}");
        long b = registers["B"];
        long c = registers["C"];
        string program = string.Join("", data.SkipWhile(l => !string.IsNullOrEmpty(l))).Split(':')[1];
        Console.WriteLine($"program: {program}");
        var ins = program.Split(',').Select(int.Parse).ToArray();
        if (Helpers.IncludeVerboseOutput)
        {
            for (int ip = 0; ip < ins.Length; ip += 2)
            {
                var code = ins[ip];
                var op = ins[ip + 1];
                string comboText = op switch
                {
                    4 => "A",
                    5 => "B",
                    6 => "C",
                    7 => "THROW",
                    var qq => qq.ToString(),
                };
                string opCode = code switch
                {
                    0 => $"A = A >> {comboText}",
                    1 => $"B = B ^ {op}",
                    2 => $"B = {comboText} % 8",
                    3 => $"jmp if A to {op}",
                    4 => "B = B ^ C",
                    5 => $"out {comboText} % 8",
                    6 => $"B = A >> {comboText}",
                    7 => $"C = A >> {comboText}",
                };
                Helpers.VerboseLine(opCode);
            }
        }

        byte firstConstant = 255;
        byte secondConstant = 255;

        Console.Write("Output: ");
        bool wrote = false;
        for (int ip = 0; ip < ins.Length; ip+=2)
        {
            var code = ins[ip];
            var op = ins[ip + 1];
            var combo = ReadCombo(op);
            switch (code)
            {
                case 0:
                    a >>= (int)combo;
                    break;
                case 1:
                    if (firstConstant == 255)
                    {
                        firstConstant = (byte)op;
                    } else if (secondConstant == 255)
                    {
                        secondConstant = (byte)op;
                    }

                    b ^= op;
                    break;
                case 2:
                    b = combo % 8;
                    break;
                case 3:
                    if (a != 0)
                    {
                        ip = op - 2;
                    }
                    break;
                case 4:
                    b ^= c;
                    break;
                case 5:
                    if (wrote)
                    {
                        Console.Write(',');
                    }
                    wrote = true;
                    Console.Write(combo % 8);
                    break;
                case 6:
                    b = a >> (int)combo;
                    break;
                case 7:
                    c = a >> (int)combo;
                    break;
            }
        }

        long ReadCombo(int value) =>
            value switch
            {
                4 => a,
                5 => b,
                6 => c,
                7 => 0,
                var x => x,
            };

        Console.WriteLine();
        var s = Stopwatch.StartNew();
        TaskBasedMemoSolver<StupidState, long> solver = new();
        var result = solver.Solve(new StupidState(0, ins.Reverse().ToImmutableList(), firstConstant, secondConstant));
        Console.WriteLine($"[TIME] {s.Elapsed}");
        Console.WriteLine($"Reflective A register: {result}");
    }

    public readonly struct StupidState(long a, ImmutableList<int> mustOutput, byte firstConstant, byte secondConstant) : ITaskMemoState<StupidState, long>, IEquatable<StupidState>
    {
        private readonly long _a = a;

        private (long a, long b) RunStep(long a)
        {
            // You'll need to tweak this to match your program, run it in verbose mose to see that the constants are and replace them.
            byte b = (byte)((a & 7) ^ firstConstant);
            long c = a >> b;
            a >>= 3;
            b = (byte)((c ^ b ^ secondConstant) & 7);
            return (a, b);
        }

        public async Task<long> Solve(IAsyncSolver<StupidState, long> solver)
        {
            if (mustOutput.IsEmpty) return _a;
            var targetB = mustOutput[0];
            var rem = mustOutput.RemoveAt(0);
            for (long i = 0; i < 32; i++)
            {
                long a = _a << 3 | i;
                (long nextA, long b) = RunStep(a);
                if (b == targetB && nextA == _a)
                {
                    // we now need the next thing to return nextA
                    var next = await solver.GetSolutionAsync(new StupidState(a, rem, firstConstant, secondConstant));
                    if (next != 0)
                    {
                        return next;
                    }
                }
            }

            return 0;
        }

        public override int GetHashCode() => _a.GetHashCode();
        public bool Equals(StupidState other) => _a == other._a;
    }
}