namespace ChadNedzlek.AdventOfCode.Library;

public readonly record struct RangeL(long Start, long Length)
{
    /// <summary>
    /// Exclusive end boundary.
    /// </summary>
    /// <example>{Start:1, End:4} => [1, 2, 3]</example>
    public long End => Start + Length;

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
}