using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;

namespace ChadNedzlek.AdventOfCode.Library;

public static class Helpers
{
    public static void Deconstruct<T>(this T[] arr, out T a, out T b)
    {
        if (arr.Length != 2)
            throw new ArgumentException($"{nameof(arr)} must be 2 elements in length", nameof(arr));
        a = arr[0];
        b = arr[1];
    }
        
    public static void Deconstruct<T>(this T[] arr, out T a, out T b, out T c)
    {
        if (arr.Length != 3)
            throw new ArgumentException($"{nameof(arr)} must be 2 elements in length", nameof(arr));
        a = arr[0];
        b = arr[1];
        c = arr[2];
    }

    public static IEnumerable<T> AsEnumerable<T>(this T[,] arr)
    {
        for (int i0 = 0; i0 < arr.GetLength(0); i0++)
        for (int i1 = 0; i1 < arr.GetLength(1); i1++)
        {
            yield return arr[i0, i1];
        }
    }

    public static void For<T>(this T[,] arr, Action<T[,], int, int, T> act)
    {
        for (int i0 = 0; i0 < arr.GetLength(0); i0++)
        for (int i1 = 0; i1 < arr.GetLength(1); i1++)
        {
            act(arr, i0, i1, arr[i0,i1]);
        }
    }

    public static void For<T>(this T[,] arr, Action<T[,], int, int> act)
    {
        For(arr, (arr, a, b, __) => act(arr, a, b));
    }

    public static void For<T>(this T[,] arr, Action<int, int> act)
    {
        For(arr, (_, a, b, __) => act(a, b));
    }
        
    public static IEnumerable<T> AsEnumerable<T>(this T value)
    {
        return Enumerable.Repeat(value, 1);
    }

    public static IEnumerable<int> AsEnumerable(this Range range)
    {
        var (start, count) = range.GetOffsetAndLength(int.MaxValue);
        return Enumerable.Range(start, count);
    }

    public static int PosMod(this int x, int q) => (x % q + q) % q;
        
    public static T Gcd<T>(T num1, T num2)
    where T: IEqualityOperators<T, T, bool>, IComparisonOperators<T,T,bool>, ISubtractionOperators<T,T,T>
    {
        while (num1 != num2)
        {
            if (num1 > num2)
                num1 = num1 - num2;
 
            if (num2 > num1)
                num2 = num2 - num1;
        }
        return num1;
    }
  
    public static T Lcm<T>(T num1, T num2) where T : IMultiplyOperators<T,T,T>, IDivisionOperators<T,T,T>, IComparisonOperators<T, T, bool>, ISubtractionOperators<T, T, T>
    {
        return (num1 * num2) / Gcd(num1, num2);
    }

    public static void AddOrUpdate<TKey, TValue>(
        this IDictionary<TKey, TValue> dict,
        TKey key,
        TValue add,
        Func<TValue, TValue> update)
    {
        if (dict.TryGetValue(key, out var existing))
        {
            dict[key] = update(existing);
        }
        else
        {
            dict.Add(key, add);
        }
    }
    public static IImmutableDictionary<TKey, TValue> AddOrUpdate<TKey, TValue>(
        this IImmutableDictionary<TKey, TValue> dict,
        TKey key,
        TValue add,
        Func<TValue, TValue> update)
    {
        if (dict.TryGetValue(key, out var existing))
        {
            return dict.SetItem(key, update(existing));
        }
        else
        {
            return dict.Add(key, add);
        }
    }
        
    public static void Increment<TKey>(
        this IDictionary<TKey, int> dict,
        TKey key,
        int amount = 1)
    {
        AddOrUpdate(dict, key, amount, i => i + amount);
    }
        
    public static void Decrement<TKey>(
        this IDictionary<TKey, int> dict,
        TKey key,
        int amount = 1)
    {
        Increment(dict, key, -amount);
    }
        
    public static void Increment<TKey>(
        this IDictionary<TKey, long> dict,
        TKey key,
        long amount = 1)
    {
        AddOrUpdate(dict, key, amount, i => i + amount);
    }

    public static IEnumerable<IEnumerable<T>> Chunks<T>(IEnumerable<T> source, int chunkSize)
    {
        using var enumerator = source.GetEnumerator();
        IEnumerable<T> Inner()
        {
            bool needToRead = false;
            for (int i = 0; i < chunkSize; i++)
            {
                if (needToRead && !enumerator.MoveNext()) yield break;

                yield return enumerator.Current;

                needToRead = true;
            }
        }

        while (enumerator.MoveNext())
        {
            yield return Inner();
        }
    }

    public static T Product<T>(this IEnumerable<T> source)
        where T : IMultiplicativeIdentity<T, T>, IMultiplyOperators<T, T, T>
        => source.Aggregate(T.MultiplicativeIdentity, (a, b) => a * b);

    public static T Lcm<T>(this IEnumerable<T> source)
        where T : IMultiplicativeIdentity<T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>,
        IComparisonOperators<T, T, bool>, ISubtractionOperators<T, T, T> =>
        source.Aggregate(T.MultiplicativeIdentity, Lcm);


    public static bool IncludeVerboseOutput { get; set; }

    public static void Verbose(string text)
    {
        if (IncludeVerboseOutput)
            Console.Write(text);
    }

    public static void VerboseLine(string line)
    {
        if (IncludeVerboseOutput)
            Console.WriteLine(line);
    }

    public static void IfVerbose(Action callback)
    {
        if (IncludeVerboseOutput)
            callback();
    }

    public static int FindIndex<T>(this IEnumerable<T> source, Predicate<T> predicate)
    {
        var i = 0;
        foreach (var item in source)
        {
            if (predicate(item))
                return i;
            i++;
        }

        return -1;
    }

    public static bool TryGet<T>(this IReadOnlyList<IReadOnlyList<T>> input, int i1, int i2, out T value)
    {
        if (i1 < 0 || i1 >= input.Count)
        {
            value = default;
            return false;
        }

        var l = input[i1];
        if (i2 < 0 || i2 >= l.Count)
        {
            value = default;
            return false;
        }

        value = l[i2];
        return true;
    }
        
    public static bool TryGet(this IReadOnlyList<string> input, int i1, int i2, out char value)
    {
        if (i1 < 0 || i1 >= input.Count)
        {
            value = default;
            return false;
        }

        string l = input[i1];
        if (i2 < 0 || i2 >= l.Length)
        {
            value = default;
            return false;
        }

        value = l[i2];
        return true;
    }

    public static bool TryGet<T>(this T[,] input, GPoint2I p, out T value) => TryGet(input, p.Row, p.Col, out value);
        
    public static bool TryGet<T>(this T[,] input, int i1, int i2, out T value)
    {
        if (i1 < 0 || i1 >= input.GetLength(0))
        {
            value = default;
            return false;
        }

        if (i2 < 0 || i2 >= input.GetLength(1))
        {
            value = default;
            return false;
        }

        value = input[i1, i2];
        return true;
    }
        
    public static bool TrySet<T>(this T[,] input, int i1, int i2, T value)
    {
        if (i1 < 0 || i1 >= input.GetLength(0))
        {
            return false;
        }
        if (i2 < 0 || i2 >= input.GetLength(1))
        {
            return false;
        }

        input[i1, i2] = value;
        return true;
    }

    public static bool IsInRange<T>(this T[,] input, GPoint2I p) => IsInRange<T>(input, p.Row, p.Col);
        
    public static bool IsInRange<T>(this T[,] input, int i1, int i2)
    {
        if (i1 < 0 || i1 >= input.GetLength(0))
        {
            return false;
        }
        if (i2 < 0 || i2 >= input.GetLength(1))
        {
            return false;
        }
        
        return true;
    }

    public static IEnumerable<int> RangeInc(int start, int end)
    {
        if (end < start)
            (end, start) = (start, end);

        if (end == start)
            return Array.Empty<int>();
        
        return Enumerable.Range(start, end - start + 1);
    }

    public static Dictionary<TSource, int> CountBy<TSource>(this IEnumerable<TSource> input) => CountBy(input, i => i);

    public static Dictionary<TResult, int> CountBy<TSource, TResult>(this IEnumerable<TSource> input, Func<TSource, TResult> selector)
    {
        return input.GroupBy(selector).ToDictionary(g => g.Key, g => g.Count());
    }

    public static IEnumerable<ImmutableList<T>> SplitWhen<T>(this IEnumerable<T> input, Func<T, bool> when, bool skipEmpty = true)
    {
        var b = ImmutableList.CreateBuilder<T>();
        foreach (var t in input)
        {
            if (when(t))
            {
                if (b.Count == 0)
                    continue;
                yield return b.ToImmutable();
                b.Clear();
                continue;
            }

            b.Add(t);
        }
        if (b.Count == 0)
            yield break;
        yield return b.ToImmutable();
    }

    public static string ReplaceOnce(this string s, char c, char r)
    {
        int i = s.IndexOf(c);
        return s[..i] + r + s[(i+1)..];
    }

    public static string RemStart(this string s, int len)
    {
        if (s.Length <= len)
            return "";
        return s[len..];
    }

    public static string RemEnd(this string s, int len)
    {
        if (s.Length <= len)
            return "";
        return s[..^len];
    }

    public static char[,] ToCharArray(this string[] input)
    {
        char[,] arr = new char[input.Length, input[0].Length];
        for (var r = 0; r < input.Length; r++)
        {
            string s = input[r];
            for (int c = 0; c < s.Length; c++)
            {
                arr[r, c] = s[c];
            }
        }

        return arr;
    }

    public static void PartialOrder<T>(List<T> bits, Func<T, T, bool> swap)
    {
        var loop = true;
        while (loop)
        {
            loop = false;
            for (int i = 0; i < bits.Count - 1; i++)
            {
                if (swap(bits[i], bits[i + 1]))
                {
                    (bits[i], bits[i + 1]) = (bits[i + 1], bits[i]);
                    loop = true;
                }
            }
        }   
    }
}