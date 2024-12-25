using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ChadNedzlek.AdventOfCode.Y2024.CSharp;

public class Problem25 : SyncProblemBase
{
    protected override void ExecuteCore(string[] data)
    {
        List<Vector<byte>> keys = [];
        List<Vector<byte>> locks = [];
        Vector<ushort> keyMask = new Vector<ushort>((ReadOnlySpan<ushort>)[1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]);
        for (int i = 0; i < data.Length; i += 8)
        {
            scoped ref List<Vector<byte>> target = ref locks;
            if (data[i] == "#####")
            {
                target = ref keys;
            }
            
            Vector<byte> building = Vector<byte>.Zero;
            for (int j = i; j < i + 7; j++)
            {
                ReadOnlySpan<char> lineSpan = data[j].AsSpan();
                var shortLine = Unsafe.ReadUnaligned<Vector<ushort>>(ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(lineSpan)));
                Vector<byte> line = Vector.Narrow(shortLine & keyMask, Vector<ushort>.Zero);
                building += line;
            }

            target.Add(building);
        }

        long count = 0;
        foreach(Vector<byte> key in keys)
        foreach (Vector<byte> @lock in locks)
        {
            var pair = key + @lock;
            bool fit = Vector.LessThanAll(pair, new Vector<byte>(8));
            if (fit) count++;
        }
        Console.WriteLine($"Fit keys: {count}");
    }
}