using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ChadNedzlek.AdventOfCode.DataModule;
using JetBrains.Annotations;

namespace ChadNedzlek.AdventOfCode.Library;

public static class Data
{
    public static async Task<string[]> GetDataAsync(int problem, bool sample = false, int year = 0)
    {
        if (!sample)
        {
            var data = new AocData(year);
            return await data.GetDataAsync(problem);
        }

        string root = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!;
        try
        {
            return await File.ReadAllLinesAsync(Path.Combine(root, "data", $"data-{problem:00}-example.txt"));
        }
        catch (FileNotFoundException)
        {
            throw new NoDataException();
        }
    }

    public static IEnumerable<ValueTuple<T1, T2>> As<T1, T2>(
        this IEnumerable<string> data,
        [RegexPattern] string pattern)
    {
        foreach (string line in data)
        {
            yield return Parse<T1, T2>(line, pattern);
        }
    }

    public static IEnumerable<ValueTuple<T1, T2, T3>> As<T1, T2, T3>(
        this IEnumerable<string> data,
        [RegexPattern] string pattern)
    {
        foreach (string line in data)
        {
            yield return Parse<T1, T2, T3>(line, pattern);
        }
    }

    public static IEnumerable<TTyped> AsTyped<T1, T2, T3, TTyped>(
        this IEnumerable<string> data,
        [RegexPattern] string pattern)
        where TTyped : IConvertable<(T1, T2, T3), TTyped>
    {
        foreach (string line in data)
        {
            yield return Parse<T1, T2, T3>(line, pattern);
        }
    }

    public static IEnumerable<ValueTuple<T1, T2, T3, T4>> As<T1, T2, T3, T4>(
        this IEnumerable<string> data,
        [RegexPattern] string pattern)
    {
        foreach (string line in data)
        {
            yield return Parse<T1, T2, T3, T4>(line, pattern);
        }
    }

    public static IEnumerable<ValueTuple<T1, T2, T3, T4, T5, T6>> As<T1, T2, T3, T4, T5, T6>(
        this IEnumerable<string> data,
        [RegexPattern] string pattern)
    {
        foreach (string line in data)
        {
            yield return Parse<T1, T2, T3, T4, T5, T6>(line, pattern);
        }
    }

    public static IEnumerable<ValueTuple<T1, T2, T3, T4, T5, T6, T7>> As<T1, T2, T3, T4, T5, T6, T7>(
        this IEnumerable<string> data,
        [RegexPattern] string pattern)
    {
        foreach (string line in data)
        {
            yield return Parse<T1, T2, T3, T4, T5, T6, T7>(line, pattern);
        }
    }


    public static IEnumerable<T1> As<T1>(
        this IEnumerable<string> data,
        [RegexPattern] string pattern)
    {
        foreach (string line in data)
        {
            yield return Parse<T1>(line, pattern);
        }
    }

    public static (T1, T2, T3, T4) Parse<T1, T2, T3, T4>(this string line, [RegexPattern] string pattern)
    {
        var m = Regex.Match(line, pattern);
        return (
            (T1)Convert.ChangeType(m.Groups[1].Value, typeof(T1)),
            (T2)Convert.ChangeType(m.Groups[2].Value, typeof(T2)),
            (T3)Convert.ChangeType(m.Groups[3].Value, typeof(T3)),
            (T4)Convert.ChangeType(m.Groups[4].Value, typeof(T4))
        );
    }

    public static (T1, T2, T3, T4, T5, T6) Parse<T1, T2, T3, T4, T5, T6>(this string line, [RegexPattern] string pattern)
    {
        var m = Regex.Match(line, pattern);
        return (
            (T1)Convert.ChangeType(m.Groups[1].Value, typeof(T1)),
            (T2)Convert.ChangeType(m.Groups[2].Value, typeof(T2)),
            (T3)Convert.ChangeType(m.Groups[3].Value, typeof(T3)),
            (T4)Convert.ChangeType(m.Groups[4].Value, typeof(T4)),
            (T5)Convert.ChangeType(m.Groups[5].Value, typeof(T5)),
            (T6)Convert.ChangeType(m.Groups[6].Value, typeof(T6))
        );
    }

    public static (T1, T2, T3, T4, T5, T6, T7) Parse<T1, T2, T3, T4, T5, T6, T7>(this string line, [RegexPattern] string pattern)
    {
        var m = Regex.Match(line, pattern);
        return (
            (T1)Convert.ChangeType(m.Groups[1].Value, typeof(T1)),
            (T2)Convert.ChangeType(m.Groups[2].Value, typeof(T2)),
            (T3)Convert.ChangeType(m.Groups[3].Value, typeof(T3)),
            (T4)Convert.ChangeType(m.Groups[4].Value, typeof(T4)),
            (T5)Convert.ChangeType(m.Groups[5].Value, typeof(T5)),
            (T6)Convert.ChangeType(m.Groups[6].Value, typeof(T6)),
            (T7)Convert.ChangeType(m.Groups[7].Value, typeof(T7))
        );
    }

    public static (T1, T2, T3) Parse<T1, T2, T3>(this string line, [RegexPattern] string pattern)
    {
        var m = Regex.Match(line, pattern);
        if (m.Success == false)
            throw new ArgumentException("Pattern does not match input line", nameof(pattern));
        return (
            (T1)Convert.ChangeType(m.Groups[1].Value, typeof(T1)),
            (T2)Convert.ChangeType(m.Groups[2].Value, typeof(T2)),
            (T3)Convert.ChangeType(m.Groups[3].Value, typeof(T3))
        );
    }

    public static (T1, T2) Parse<T1, T2>(this string line, [RegexPattern] string pattern)
    {
        var m = Regex.Match(line, pattern);
        return (
            (T1)Convert.ChangeType(m.Groups[1].Value, typeof(T1)),
            (T2)Convert.ChangeType(m.Groups[2].Value, typeof(T2))
        );
    }

    public static T1 Parse<T1>(this string line, [RegexPattern] string pattern)
    {
        var m = Regex.Match(line, pattern);
        return (T1)Convert.ChangeType(m.Groups[1].Value, typeof(T1));
    }
}