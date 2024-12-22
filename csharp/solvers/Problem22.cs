using System;
using System.Buffers;
using System.Linq;
using ChadNedzlek.AdventOfCode.Library;

namespace ChadNedzlek.AdventOfCode.Y2024.CSharp;

public class Problem22 : SyncProblemBase
{
    protected override void ExecuteCore(string[] data)
    {
        long sum = 0;
        foreach (var v in data.As<long>("(.*)"))
        {
            long value = v;
            for (int i = 0; i < 2000; i++)
            {
                value = Evolve(value);
            }

            sum += value;
        }
        Console.WriteLine($"Value: {sum}");
        sbyte[,] prices = new sbyte[data.Length,2000];
        sbyte[,] changes = new sbyte[data.Length,2000];
        long[] numbers = data.As<long>("(.*)").ToArray();
        for (var l = 0; l < numbers.Length; l++)
        {
            long value = numbers[l];
            prices[l,0] = (sbyte)(value % 10);
            for (int i = 1; i < 2000; i++)
            {
                value = Evolve(value);
                prices[l,i] =  (sbyte)(value % 10);
                changes[l,i] = (sbyte)(prices[l,i] - prices[l,i - 1]);
            }
        }

        var priceSpan = prices.AsFlatSpan();
        var deltaSpan = changes.AsFlatSpan();
        Span<sbyte> target = stackalloc sbyte[4];
        target.Fill(-9);
        long bestBananas = 0;
        for(target[0] = -9; target[0] <= 9; target[0]++)
        for(target[1] = -9; target[1] <= 9; target[1]++)
        for(target[2] = -9; target[2] <= 9; target[2]++)
        for(target[3] = -9; target[3] <= 9; target[3]++)
        {
            if (sbyte.Abs((sbyte)(target[0] + target[1])) > 9 ||
                sbyte.Abs((sbyte)(target[1] + target[2])) > 9 ||
                sbyte.Abs((sbyte)(target[2] + target[3])) > 9)
            {
                continue;
            }

            long bananas = 0;
            var t = deltaSpan.IndexOf(target);
            while (t != -1)
            {
                int wait = t % 2000;
                if (wait + 3 < 2000)
                {
                    bananas += priceSpan[t + 3];
                }

                int nextStart = t - wait + 2000;
                int inSpan = deltaSpan[nextStart..].IndexOf(target);
                if (inSpan == -1)
                    break;
                t = nextStart + inSpan;
            }

            bestBananas = long.Max(bestBananas, bananas);
        }
        Console.WriteLine($"Bananas: {bestBananas}");
    }

    private long Evolve(long n)
    {
        n = Prune(Mix(n, n << 6));
        n = Prune(Mix(n, n >> 5));
        n = Prune(Mix(n, n << 11));
        return n;
    }

    private long Mix(long n, long x) => n ^ x;

    private long Prune(long n) => n & 0xFFFFFF;
}