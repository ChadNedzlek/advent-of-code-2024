using ChadNedzlek.AdventOfCode.Library;
using FluentAssertions;

namespace csharp.Tests;

public class AlgorithmTests
{
    [TestCase(4, 4, 1)]
    [TestCase(5, 4, 4)]
    [TestCase(6, 4, 10)]
    [TestCase(7, 5, 15)]
    [TestCase(9, 4, 56)]
    public void Distribute(int count, int buckets, int total)
    {
        int i = 0;
        foreach (var x in Algorithms.Distribute(count, buckets))
        {
            x.Sum().Should().Be(count);
            x.Should().AllSatisfy(v => v.Should().BeGreaterThan(0));
            i++;
            i.Should().BeLessThan(100_000_000);
        }

        i.Should().Be(total);
    }
    
    [TestCase(29, 11, 56)]
    public void BigDistribute(int count, int buckets, int total)
    {
        int i = 0;
        foreach (var x in Algorithms.Distribute(count, buckets))
        {
            i++;
            i.Should().BeLessThan(100_000_000);
        }

        i.Should().Be(total);
    }
}