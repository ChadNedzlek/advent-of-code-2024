using System;
using System.Collections.Immutable;
using System.Linq;
using ChadNedzlek.AdventOfCode.Library;

namespace ChadNedzlek.AdventOfCode.Y2024.CSharp;

public class Problem10 : SyncProblemBase
{
    protected override void ExecuteCore(string[] data)
    {
        ImmutableHashSet<GPoint2<int>>[,] nonUniqueScore = data.Select2D(_ => ImmutableHashSet<GPoint2<int>>.Empty);
        foreach (var x in data.AsEnumerableWithPoint())
        {
            if (x.value != '9') continue;
            nonUniqueScore.TrySet(x.point, [x.point]);
        }

        for (int i = 8; i >= 0; i--)
        {
            foreach (var x in data.AsEnumerableWithPoint())
            {
                if (x.value != '0' + i) continue;
                nonUniqueScore.TrySet(x.point, Helpers.OrthogonalDirections.Aggregate(
                    ImmutableHashSet<GPoint2<int>>.Empty, 
                    (a, d) => data.Get(x.point + d) == '0' + i + 1 ? a.Union(nonUniqueScore.Get(x.point + d)) : a));
            }
        }

        var nonUnique = data.AsEnumerableWithPoint().Where(x => x.value == '0').Sum(x => nonUniqueScore.Get(x.point).Count);
        Console.WriteLine($"Count of heads : {nonUnique}");
            
        int[,] score = data.Select2D(_ => -1);
        foreach (var x in data.AsEnumerableWithPoint())
        {
            if (x.value != '9') continue;
            score.TrySet(x.point, 1);
        }
            
        foreach (var x in data.AsEnumerableWithPoint())
        {
            if (x.value != '9') continue;
            score.TrySet(x.point, 1);
        }

        for (int i = 8; i >= 0; i--)
        {
            foreach (var x in data.AsEnumerableWithPoint())
            {
                if (x.value != '0' + i) continue;
                score.TrySet(x.point, Helpers.OrthogonalDirections.Sum(d => data.Get(x.point + d) == '0' + i + 1 ? score.Get(x.point + d) : 0));
            }
        }

        var total = data.AsEnumerableWithPoint().Where(x => x.value == '0').Sum(x => score.Get(x.point));
        Console.WriteLine($"Count of trails : {total}");
    }
}