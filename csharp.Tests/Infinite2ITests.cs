using ChadNedzlek.AdventOfCode.Library;
using ChadNedzlek.AdventOfCode.Y2023.CSharp;
using FluentAssertions;

namespace csharp.Tests;

public class Infinite2ITests
{
    [Test]
    public void TrySetOutOfBoundsDoesNotSet()
    {
        Infinite2I<int> value = new Infinite2I<int>(10, 10);
        value.TrySet(100, 5, -999);
        value.GetUpperBound(0).Should().Be(9);
        value.GetUpperBound(1).Should().Be(9);
        value.GetLength(0).Should().Be(10);
        value.GetLength(1).Should().Be(10);
        value.GetLowerBound(0).Should().Be(0);
        value.GetLowerBound(1).Should().Be(0);
        value[100, 5].Should().Be(default);
    }
    
    [Test]
    public void SetXLargeIncreases()
    {
        Infinite2I<int> value = new Infinite2I<int>(10, 10);
        value[100, 5] = -999;
        value.GetUpperBound(0).Should().Be(100);
        value.GetUpperBound(1).Should().Be(9);
        value.GetLength(0).Should().Be(101);
        value.GetLength(1).Should().Be(10);
        value.GetLowerBound(0).Should().Be(0);
        value.GetLowerBound(1).Should().Be(0);
        value[100, 5].Should().Be(-999);
    }
    
    [Test]
    public void SetYLargeIncreases()
    {
        Infinite2I<int> value = new Infinite2I<int>(10, 10);
        value[5, 100] = -999;
        value.GetUpperBound(0).Should().Be(9);
        value.GetUpperBound(1).Should().Be(100);
        value.GetLength(0).Should().Be(10);
        value.GetLength(1).Should().Be(101);
        value.GetLowerBound(0).Should().Be(0);
        value.GetLowerBound(1).Should().Be(0);
        value[5,100].Should().Be(-999);
    }
    
    [Test]
    public void SetNegativeXIncreased()
    {
        Infinite2I<int> value = new Infinite2I<int>(10, 10);
        value[-5, 5] = -999;
        value.GetLowerBound(0).Should().Be(-5);
        value.GetUpperBound(0).Should().Be(9);
        value.GetLength(0).Should().Be(15);
        
        value.GetLowerBound(1).Should().Be(0);
        value.GetUpperBound(1).Should().Be(9);
        value.GetLength(1).Should().Be(10);
        
        value[-5, 5].Should().Be(-999);
    }

    [Test]
    public void SetUpdates()
    {
        Infinite2I<int> value = new Infinite2I<int>(10, 10);
        value[5, 5].Should().Be(0);
        value[5, 5] = -999;
        value[5, 5].Should().Be(-999);
        value.TrySet(5, 5, 500);
        value[5, 5].Should().Be(500);
    }
    
}