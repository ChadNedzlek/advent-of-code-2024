using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using ChadNedzlek.AdventOfCode.Library;
using TorchSharp.Modules;

namespace ChadNedzlek.AdventOfCode.Y2024.CSharp;

public class Problem24 : SyncProblemBase
{
    protected override void ExecuteCore(string[] data)
    {
        var start = data.TakeWhile(d => !string.IsNullOrWhiteSpace(d)).Select(s => s.Split(':')).ToDictionary(s => s[0], s => ulong.Parse(s[1]));
        var gates = data.SkipWhile(d => !string.IsNullOrWhiteSpace(d))
            .Skip(1)
            .Where(d => !string.IsNullOrWhiteSpace(d))
            .Select(x => x.Split(' '))
            .Select(x => new Gate(x[0], x[2], x[1], x[4]))
            .ToImmutableDictionary(x => x.Destination);

        ulong output = RunLogic(gates, start);
        Console.WriteLine($"Value: {output}");
        Dictionary<string, string> gateSwaps = [];
        ImmutableList<Gate> corrected = BuildCorrectGates(gates.Values.ToImmutableList(), gateSwaps);

        var result = RunLogic(corrected.ToImmutableDictionary(g => g.Destination), 0, 123456, 45);
        if (result != 45)
        {
        }

        Console.WriteLine("After building a valid adder, the following gates are wrong:");
        Console.WriteLine(string.Join(',', gateSwaps.Keys.Concat(gateSwaps.Values).OrderBy()));
    }

    private ImmutableList<Gate> BuildCorrectGates(
        ImmutableList<Gate> gates,
        Dictionary<string, string> gateSwaps,
        int bits = -1
    )
    {
        if (bits == -1)
        {
            bits = gates.Where(g => g.Destination[0] == 'z').Max(g => int.Parse(g.Destination[1..]));
        }

        return BuildCorrectGates(gates, gateSwaps, bits - 1, out _);
    }

    private ImmutableList<Gate> BuildCorrectGates(
        ImmutableList<Gate> gates,
        Dictionary<string, string> gateSwaps,
        int bits,
        out Gate carryGate
    )
    {
        if (bits == 0)
        {
            var zeroGates = gates.Where(g => g.HasInput("x00") && g.HasInput("y00")).ToDictionary(x => x.Operation);
            carryGate = zeroGates["AND"];
            var sum = zeroGates["XOR"];

            if (sum.Destination != "z00")
            {
                Console.WriteLine($"z00 should swap with {sum.Destination}");
                gateSwaps.Add("z00", sum.Destination);
                gates = gates.Select(g => g.Swap("z00", sum.Destination)).ToImmutableList();
                carryGate = carryGate.Swap("z00", sum.Destination);
                sum = sum.Swap("z00", sum.Destination);
            }
            
            return gates;
        }

        {
            gates = BuildCorrectGates(gates, gateSwaps, bits - 1, out var lowerCarryGate);
            var partialBits = gates.Where(g => g.HasInput($"x{bits:D2}") && g.HasInput($"y{bits:D2}")).ToDictionary(x => x.Operation);
            var partialCarry = partialBits["AND"];
            var partialSum = partialBits["XOR"];
            var sum = FindOrCorrect(ref gates, partialSum.Destination, lowerCarryGate.Destination, "XOR", gateSwaps);
            var sumCarry = FindOrCorrect(ref gates, partialSum.Destination, lowerCarryGate.Destination, "AND", gateSwaps);
            if (partialCarry == null)
            {
                // The final bit won't carry (because this adder doesn't do overlfow)
                carryGate = null;
                return gates;
            }

            carryGate = FindOrCorrect(ref gates, partialCarry.Destination, sumCarry.Destination, "OR", gateSwaps);
            return gates;
        }
    }

    private Gate FindOrCorrect(ref ImmutableList<Gate> gates, string inputA, string inputB, string operation, Dictionary<string,string> swaps)
    {
        var a = gates.FirstOrDefault(g => g.Operation == operation && g.HasInput(inputA));
        if (a == null)
        {
            var b = gates.FirstOrDefault(g => g.Operation == operation && g.HasInput(inputB));
            var o = b.OtherInput(inputB);
            Console.WriteLine($"Should swap {inputA} and {o}");
            swaps.Add(inputA, o);
            gates = gates.Select(g => g.Swap(inputA, o)).ToImmutableList();
            return b.Swap(inputA, o);
        }

        if (!a.HasInput(inputB))
        {
            var o = a.OtherInput(inputA);
            Console.WriteLine($"Should swap {inputB} and {o}");
            swaps.Add(inputB, o);
            gates = gates.Select(g => g.Swap(inputB, o)).ToImmutableList();
            return a.Swap(inputB, o);
        }

        return a;
    }

    private static ulong RunLogic(ImmutableDictionary<string, Gate> gates, ulong x, ulong y, int bits)
    {
        Dictionary<string, ulong> state = [];
        for (int i = 0; i < bits; i++)
        {
            state.Add($"x{i:D2}", x & 1);
            state.Add($"y{i:D2}", y & 1);
            x >>= 1;
            y >>= 1;
        }

        return RunLogic(gates, state);
    }

    private static ulong RunLogic(ImmutableDictionary<string, Gate> gates, Dictionary<string, ulong> start)
    {
        Dictionary<string, ulong> finalOutput = [];

        ulong GetOutput(string gate)
        {
            if (finalOutput.TryGetValue(gate, out var v)) return v;
            if (gates.TryGetValue(gate, out var operation))
            {
                var a = GetOutput(operation.A);
                var b = GetOutput(operation.B);
                var d = operation.Operation switch
                {
                    "XOR" => a ^ b,
                    "AND" => a & b,
                    "OR" => a | b,
                };
                finalOutput.Add(gate, d);
                return d;
            }

            if (start.TryGetValue(gate, out v)) return v;

            return 0;
        }

        ulong output = 0;
        for (int i = 63; i >= 0; i--)
        {
            output = (output << 1) | GetOutput($"z{i:D2}");
        }

        return output;
    }

    public record Gate(string A, string B, string Operation, string Destination)
    {
        public bool HasInput(string i) => A == i || B == i;
        
        public string OtherInput(string i) => A == i ? B : A;

        public Gate Rename(string f, string t)
        {
            if (A != f && B != f && Destination != f) return this;
            
            return new Gate(A == f ? t : A, B == f ? t : B, Operation, Destination == f ? t : Destination);
        }

        public Gate Swap(string a, string b)
        {
            if (A != a && B != a && Destination != a && A != b && B != b && Destination != b) return this;
            return new Gate(
                A == a ? b : A == b ? a : A,
                B == a ? b : B == b ? a : B,
                Operation,
                Destination == a ? b : Destination == b ? a : Destination
            );
        }
    }
}