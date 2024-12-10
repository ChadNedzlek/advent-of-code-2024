using System;

namespace ChadNedzlek.AdventOfCode.Library;

public readonly struct RangeL
{
    public RangeL(long start, long length)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(length);
        Start = start;
        Length = length;
    }

    /// <summary>
    /// Exclusive end boundary.
    /// </summary>
    /// <example>{Start:1, End:4} => [1, 2, 3]</example>
    public long End => Start + Length;

    public long Start { get; init; }
    public long Length { get; init; }

    public override string ToString() => $"{Start}-{End}";

    public void Splice(RangeL spliceRange, out RangeL? before, out RangeL? mid, out RangeL? after)
    {
        if (End < spliceRange.Start || Start >= spliceRange.End)
        {
            before = this;
            mid = after = null;
            return;
        }

        before = after = null;
        RangeL cur = this;
        if (cur.Start < spliceRange.Start)
        {
            before = cur with { Length = spliceRange.Start - cur.Start };
            cur = spliceRange with { Length = cur.End - spliceRange.Start };
        }
            
        if (cur.End > spliceRange.End)
        {
            after = new RangeL(spliceRange.End, cur.End - spliceRange.End);
            cur = cur with { Length = spliceRange.End - cur.Start };
        }

        mid = cur;
    }

    public bool Contains(long value) => Start <= value && value < End;

    public static RangeL FromEnd(int end, int length) => new RangeL(end - length, length);

    public RangeL FromEnd(long length, out RangeL? remaining)
    {
        if (Length <= length)
        {
            remaining = null;
            return this;
        }

        remaining = this with { Length = Length - length };
        return new RangeL(End - length, length);
    }

    public RangeL FromStart(long length, out RangeL? remaining)
    {
        if (Length <= length)
        {
            remaining = null;
            return this;
        }

        remaining = new RangeL(Start + length, Length - length);
        return this with { Length = length };
    }

    public void Deconstruct(out long start, out long length)
    {
        start = Start;
        length = Length;
    }
}