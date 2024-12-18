using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Versioning;
using ChadNedzlek.AdventOfCode.Library;
using ComputeSharp;
using TorchSharp;

namespace ChadNedzlek.AdventOfCode.Y2024.CSharp;

public class Problem10 : SyncProblemBase
{
    protected override void ExecuteCore(string[] data)
    {
        var s = Stopwatch.StartNew();
        BasicArrayImpl(data);
        Console.WriteLine($"[TIME] Array impl: {s.Elapsed}");
        if (OperatingSystem.IsWindows())
        {
            s.Restart();
            ShaderImpl(data);
            Console.WriteLine($"[TIME] Shader impl: {s.Elapsed}");
        }
        s.Restart();
        TensorImpl(data);
        Console.WriteLine($"[TIME] Shader impl: {s.Elapsed}");
    }

    private static void BasicArrayImpl(string[] data)
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
                nonUniqueScore.TrySet(
                    x.point,
                    Helpers.OrthogonalDirections.Aggregate(
                        ImmutableHashSet<GPoint2<int>>.Empty,
                        (a, d) => data.Get(x.point + d) == '0' + i + 1 ? a.Union(nonUniqueScore.Get(x.point + d)) : a
                    )
                );
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

    [SupportedOSPlatform("windows")]
    private void ShaderImpl(string[] data)
    {
        var g = GraphicsDevice.GetDefault();
        int width = data[0].Length;
        int height = data.Length;
        ReadOnlyTexture2D<int> heights = g.AllocateReadOnlyTexture2D(
            data.AsEnumerableWithIndex().Select((c, _, _) => c - '0').ToArray(),
            width,
            height
        );
        ReadWriteTexture2D<int> sums = g.AllocateReadWriteTexture2D<int>(width, height);
        g.For(width, height, new OneFill(sums));
        g.For(width, height, new FilterToCurrentValue(sums, heights, 9));
        var neighbors = new AddNeighbors(sums);
        for (int i = 8; i >= 0; i--)
        {
            g.For(width, height, neighbors);
            g.For(width, height, new FilterToCurrentValue(sums, heights, i));
        }
        g.For(width, height, new FilterToCurrentValue(sums, heights, 0));
        var sum = sums.ToArray().AsEnumerable().Sum();
        Console.WriteLine($"Shader sum: {sum}");
    }

    private void TensorImpl(string[] data)
    {
        var device = torch.cuda.is_available() ? torch.CUDA : torch.mps_is_available() ? torch.MPS : torch.CPU;
    }
}

[ThreadGroupSize(DefaultThreadGroupSizes.XY)]
[GeneratedComputeShaderDescriptor]
[SupportedOSPlatform("windows")]
public readonly partial struct AddNeighbors(ReadWriteTexture2D<int> sums) : IComputeShader
{
    public void Execute()
    {
        sums[ThreadIds.XY] = sums[ThreadIds.X - 1, ThreadIds.Y] +
            sums[ThreadIds.X + 1, ThreadIds.Y] +
            sums[ThreadIds.X, ThreadIds.Y - 1] +
            sums[ThreadIds.X, ThreadIds.Y + 1];
    }
}

[ThreadGroupSize(DefaultThreadGroupSizes.XY)]
[GeneratedComputeShaderDescriptor]
[SupportedOSPlatform("windows")]
public readonly partial struct FilterToCurrentValue(ReadWriteTexture2D<int> sums, ReadOnlyTexture2D<int> heights, int targetHeight) : IComputeShader
{
    public void Execute()
    {
        int mask = Hlsl.BoolToInt(heights[ThreadIds.XY] != targetHeight) - 1;
        sums[ThreadIds.XY] = mask & sums[ThreadIds.XY];
    }
}
[ThreadGroupSize(DefaultThreadGroupSizes.XY)]
[GeneratedComputeShaderDescriptor]
[SupportedOSPlatform("windows")]
public readonly partial struct OneFill(ReadWriteTexture2D<int> target) : IComputeShader
{
    public void Execute()
    {
        target[ThreadIds.XY] = 1;
    }
}