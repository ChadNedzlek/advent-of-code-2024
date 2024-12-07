using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

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
        int length0 = arr.GetLength(0);
        int length1 = arr.GetLength(1);
        for (int i0 = 0; i0 < length0; i0++)
        for (int i1 = 0; i1 < length1; i1++)
        {
            yield return arr[i0, i1];
        }
    }

    public static IEnumerable<(T value, int index0, int index1)> AsEnumerableWithIndex<T>(this T[,] arr)
    {
        int length0 = arr.GetLength(0);
        int length1 = arr.GetLength(1);
        for (int i0 = 0; i0 < length0; i0++)
        for (int i1 = 0; i1 < length1; i1++)
        {
            yield return (arr[i0, i1], i0, i1);
        }
    }

    public static IEnumerable<(T value, int index0, int index1)> AsEnumerableWithIndex<T>(this IReadOnlyList<IReadOnlyList<T>> arr)
    {
        for (int i0 = 0; i0 < arr.Count; i0++)
        for (int i1 = 0; i1 < arr[0].Count; i1++)
        {
            yield return (arr[i0][i1], i0, i1);
        }
    }

    public static IEnumerable<(char value, int index0, int index1)> AsEnumerableWithIndex(this IReadOnlyList<string> arr)
    {
        for (int i0 = 0; i0 < arr.Count; i0++)
        for (int i1 = 0; i1 < arr[0].Length; i1++)
        {
            yield return (arr[i0][i1], i0, i1);
        }
    }

    public static void For<T>(this T[,] arr, Action<T[,], int, int, T> act)
    {
        int length0 = arr.GetLength(0);
        int length1 = arr.GetLength(1);
        for (int i0 = 0; i0 < length0; i0++)
        for (int i1 = 0; i1 < length1; i1++)
        {
            act(arr, i0, i1, arr[i0, i1]);
        }
    }

    public static void For<T>(this T[,] arr, Action<T[,], int, int> act)
    {
        For(arr, (a, i0, i1, __) => act(a, i0, i1));
    }

    public static void For<T>(this T[,] arr, Action<int, int> act)
    {
        For(arr, (_, a, b, __) => act(a, b));
    }

    public static IEnumerable<T> AsEnumerable<T>(this T value) => [value];

    public static IEnumerable<int> AsEnumerable(this Range range)
    {
        var (start, count) = range.GetOffsetAndLength(int.MaxValue);
        return Enumerable.Range(start, count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T PosMod<T>(this T x, T q)
        where T : IModulusOperators<T, T, T>, IAdditionOperators<T, T, T>
        => (x % q + q) % q;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Gcd<T>(T num1, T num2)
        where T : IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, ISubtractionOperators<T, T, T>
    {
        while (num1 != num2)
        {
            if (num1 > num2)
                num1 -= num2;

            if (num2 > num1)
                num2 -= num1;
        }

        return num1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Lcm<T>(T num1, T num2)
        where T : IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IComparisonOperators<T, T, bool>, ISubtractionOperators<T, T, T>
    {
        return (num1 * num2) / Gcd(num1, num2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddOrUpdate<TKey, TValue>(
        this IDictionary<TKey, TValue> dict,
        TKey key,
        TValue add,
        Func<TValue, TValue> update
    )
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IImmutableDictionary<TKey, TValue> AddOrUpdate<TKey, TValue>(
        this IImmutableDictionary<TKey, TValue> dict,
        TKey key,
        TValue add,
        Func<TValue, TValue> update
    )
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Increment<TKey, TValue>(
        this IDictionary<TKey, TValue> dict,
        TKey key,
        TValue? amount = default
    )
        where TValue : struct, IAdditionOperators<TValue, TValue, TValue>, IMultiplicativeIdentity<TValue, TValue>
    {
        TValue a = amount.GetValueOrDefault(TValue.MultiplicativeIdentity);
        dict.AddOrUpdate(key, a, c => c + a);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Decrement<TKey, TValue>(
        this IDictionary<TKey, TValue> dict,
        TKey key,
        TValue? amount = default
    )
        where TValue : struct, ISubtractionOperators<TValue, TValue, TValue>, IMultiplicativeIdentity<TValue, TValue>
    {
        TValue a = amount.GetValueOrDefault(TValue.MultiplicativeIdentity);
        dict.AddOrUpdate(key, a, c => c - a);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Increment<TKey>(
        this IDictionary<TKey, long> dict,
        TKey key,
        long amount = 1
    )
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Product<T>(this IEnumerable<T> source)
        where T : IMultiplicativeIdentity<T, T>, IMultiplyOperators<T, T, T>
        => source.Aggregate(T.MultiplicativeIdentity, (a, b) => a * b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Get<T>(this IReadOnlyList<IReadOnlyList<T>> input, int i1, int i2, T defaultValue = default)
        => TryGet(input, i1, i2, out var value) ? value : defaultValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static char Get(this IReadOnlyList<string> input, int i1, int i2, char defaultValue = default)
        => TryGet(input, i1, i2, out var value) ? value : defaultValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static char Get(this IReadOnlyList<string> input, GPoint2<int> p, char defaultValue = default)
        => TryGet(input, p.Row, p.Col, out var value) ? value : defaultValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryGet<T>(this T[,] input, GPoint2<int> p, out T value) => TryGet(input, p.Row, p.Col, out value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Get<T>(this T[,] input, GPoint2<int> p, T defaultValue = default)
        => TryGet(input, p, out var value) ? value : defaultValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Get<T>(this T[,] input, int i1, int i2, T defaultValue = default)
        => TryGet(input, i1, i2, out var value) ? value : defaultValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TrySet<T>(this T[,] input, GPoint2<int> p, T value) => TrySet(input, p.Row, p.Col, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsInRange<T>(this T[,] input, GPoint2<int> p) => IsInRange<T>(input, p.Row, p.Col);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    public static readonly ImmutableArray<GPoint2<int>> EightDirections = [(-1, 0), (-1, 1), (0, 1), (1, 1), (1, 0), (1, -1), (0, -1), (-1, -1)];
    public static readonly ImmutableArray<GPoint2<int>> OrthogonalDirections = [(-1, 0), (0, 1), (1, 0), (0, -1)];
    public static readonly ImmutableArray<GPoint2<int>> DiagonalDirections = [(-1, 1), (1, 1), (1, -1), (-1, -1)];

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
        return s[..i] + r + s[(i + 1)..];
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

    public static void PartialOrder<T>(IList<T> bits, Func<T, T, bool> swap)
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

    public static IOrderedEnumerable<T> OrderBy<T>(this IEnumerable<T> source) => source.OrderBy(x => x);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TOut Into<T, TOut>(this T input, [InstantHandle] Func<T, TOut> translate) => translate(input);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Do<T>(this T input, [InstantHandle] Action<T> act) => act(input);

    public static IReadOnlyList<IReadOnlyList<T>> Select<T>(this IReadOnlyList<string> source, Func<int, int, char, T> selector)
    {
        List<List<T>> l = new List<List<T>>();

        for (int r = 0; r < source.Count; r++)
        {
            l.Add(source[r].Select((v, i) => selector(r, i, v)).ToList());
        }

        return l;
    }

    public static IReadOnlyList<IReadOnlyList<T>> Select<T>(this IReadOnlyList<string> source, Func<char, T> selector)
        => Select(source, (_, _, v) => selector(v));

    public static T[,] Select2D<T>(this IReadOnlyList<string> source, Func<int, int, char, T> selector)
    {
        T[,] res = new T[source.Count, source[0].Length];

        for (int r = 0; r < source.Count; r++)
        {
            var row = source[r];
            for (int c = 0; c < row.Length; c++)
            {
                res[r, c] = selector(r, c, row[c]);
            }
        }

        return res;
    }

    public static TOut[,] Select2D<TIn, TOut>(this TIn[,] source, Func<int, int, TIn, TOut> selector)
    {
        int length0 = source.GetLength(0);
        int length1 = source.GetLength(1);
        TOut[,] res = new TOut[length0, length1];

        for (int r = 0; r < length0; r++)
        {
            for (int c = 0; c < length1; c++)
            {
                res[r, c] = selector(r, c, source[r, c]);
            }
        }

        return res;
    }

    public static T[,] Select2D<T>(this IReadOnlyList<string> source, Func<char, T> selector) => Select2D(source, (_, _, v) => selector(v));

    public static IEnumerable<TOut> Select<T1, T2, T3, TOut>(this IEnumerable<ValueTuple<T1, T2, T3>> source, Func<T1, T2, T3, TOut> selector) =>
        source.Select(x => selector(x.Item1, x.Item2, x.Item3));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Sum<T>(this IEnumerable<T> source)
        where T : IAdditionOperators<T, T, T>, IAdditiveIdentity<T, T> => source.Aggregate(T.AdditiveIdentity, (a, v) => a + v);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TOut Sum<TIn, TOut>(this IEnumerable<TIn> source, Func<TIn, TOut> selector)
        where TOut : IAdditionOperators<TOut, TOut, TOut>, IAdditiveIdentity<TOut, TOut> =>
        source.Aggregate(TOut.AdditiveIdentity, (a, v) => a + selector(v));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (TOut1, TOut2) Sum<TIn, TOut1, TOut2>(this IEnumerable<TIn> source, Func<TIn, (TOut1, TOut2)> selector)
        where TOut1 : IAdditionOperators<TOut1, TOut1, TOut1>, IAdditiveIdentity<TOut1, TOut1>
        where TOut2 : IAdditionOperators<TOut2, TOut2, TOut2>, IAdditiveIdentity<TOut2, TOut2> =>
        source.Aggregate(
            (TOut1.AdditiveIdentity, TOut2.AdditiveIdentity),
            (a, v) => selector(v).Into(tuple => (tuple.Item1 + a.Item1, tuple.Item2 + a.Item2))
        );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue toAdd)
    {
        if (dict.TryGetValue(key, out var e)) return e;
        dict.Add(key, toAdd);
        return toAdd;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, Func<TKey, TValue> toAdd)
    {
        if (dict.TryGetValue(key, out var e)) return e;
        e = toAdd(key);
        dict.Add(key, e);
        return e;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[,] WithValueSet<T>(this T[,] input, GPoint2<int> p, T value)
    {
        T[,] ret = (T[,])input.Clone();
        ret[p.Row, p.Col] = value;
        return ret;
    }

    public static T Pow<T>(this T value, int exp)
        where T : IMultiplyOperators<T, T, T>, IAdditiveIdentity<T, T>, IMultiplicativeIdentity<T,T>
    {
        if (exp == 0)
            return T.MultiplicativeIdentity;
        var total = value;
        for (int i = 1; i < exp; i++)
        {
            total *= value;
        }

        return total;
    }
}