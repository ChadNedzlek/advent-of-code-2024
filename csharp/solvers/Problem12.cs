using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Versioning;
using ChadNedzlek.AdventOfCode.Library;
using ComputeSharp;
using Spectre.Console;
using TorchSharp;

namespace ChadNedzlek.AdventOfCode.Y2024.CSharp;

public partial class Problem12 : SyncProblemBase
{
    protected override void ExecuteCore(string[] data)
    {
        int[,] ids = data.Select2D(_ => -1);

        int idCount = 0;

        int cRows = data.Length;
        int cCols = data[0].Length;
        for (int r = 0; r < cRows; r++)
        {
            for (int c = 0; c < cCols; c++)
            {
                if (ids[r, c] == -1)
                {
                    MarkRegion((r, c), idCount++, data[r][c]);
                }
            }
        }

        Dictionary<int, int> area = [];
        Dictionary<int, int> border = [];
        Dictionary<int, char> letters = [];
        for (int r = 0; r < cRows; r++)
        for (int c = 0; c < cCols; c++)
        {
            var p = new GPoint2<int>(r, c);
            int id = ids.Get(p);
            area.Increment(id, 1);
            border.Increment(id, Helpers.OrthogonalDirections.Count(d => ids.Get(d + p) != id));
            letters.TryAdd(id, data[r][c]);
        }

        int[,] corners;
        var sw = Stopwatch.StartNew();
        {
            Console.Write("CPU: ");
            corners = GetCornersCpu(ids, idCount);
            Console.WriteLine($" {sw.Elapsed}");
        }
        
        if (OperatingSystem.IsWindows())
        {
            sw.Restart();
            Console.Write("CUDA: ");
            corners = GetCornersCuda(ids, idCount);
            Console.WriteLine($" {sw.Elapsed}");
        }

        Dictionary<int, int> cornerCounts = [];

        for (int r = 0; r < cRows; r++)
        for (int c = 0; c < cCols; c++)
        {
            cornerCounts.Increment(ids[r, c], corners[r, c]);
        }

        int[] allIds = area.Keys.ToArray();
        foreach (int id in allIds)
        {
            Helpers.VerboseLine($"A region of {letters[id]} plants with area: {area[id]}, border: {border[id]}, sides(corners): {cornerCounts[id]}");
        }
        Console.WriteLine($"Part 1 = {allIds.Sum(i => area[i] * border[i])}");
        Console.WriteLine($"Part 2 = {allIds.Sum(i => area[i] * cornerCounts[i])}");

        void MarkRegion(GPoint2<int> p, int id, char c)
        {
            if (ids.Get(p, -1) != -1 || data.Get(p) != c)
            {
                return;
            }

            if (ids.TrySet(p, id))
            {
                foreach (GPoint2<int> neighbor in Helpers.OrthogonalDirections)
                {
                    MarkRegion(p + neighbor, id, c);
                }
            }
        }
    }
    
    [ThreadGroupSize(DefaultThreadGroupSizes.XYZ)]
    [GeneratedComputeShaderDescriptor]
    [SupportedOSPlatform("windows")]
    internal readonly partial struct InflateTextureShader(ReadOnlyTexture2D<int> ids, ReadWriteTexture3D<int> inflate) : IComputeShader
    {
        public void Execute()
        {
            inflate[ThreadIds.XYZ] = Hlsl.BoolToInt(ids[ThreadIds.XY / 2] == ThreadIds.Z);
        }
    }
    
    [ThreadGroupSize(DefaultThreadGroupSizes.XYZ)]
    [GeneratedComputeShaderDescriptor]
    [SupportedOSPlatform("windows")]
    internal readonly partial struct CornerDetectShader(ReadWriteTexture3D<int> inflate) : IComputeShader
    {
        public void Execute()
        {
            var slice = new int3x3(
                inflate[ThreadIds.X - 1, ThreadIds.Y - 1, ThreadIds.Z], inflate[ThreadIds.X, ThreadIds.Y - 1, ThreadIds.Z], inflate[ThreadIds.X + 1, ThreadIds.Y - 1, ThreadIds.Z],
                inflate[ThreadIds.X - 1, ThreadIds.Y,     ThreadIds.Z], inflate[ThreadIds.XYZ],                             inflate[ThreadIds.X + 1, ThreadIds.Y,     ThreadIds.Z],
                inflate[ThreadIds.X - 1, ThreadIds.Y + 1, ThreadIds.Z], inflate[ThreadIds.X, ThreadIds.Y + 1, ThreadIds.Z], inflate[ThreadIds.X + 1, ThreadIds.Y + 1, ThreadIds.Z]
                );

            Int3 left = slice[MatrixIndex.M11, MatrixIndex.M21, MatrixIndex.M31];
            Int3 right = slice[MatrixIndex.M13, MatrixIndex.M23, MatrixIndex.M33];
            Int3 hGrad = Hlsl.Abs(right - left);
            int hGradMagnitude = hGrad[0] + hGrad[1] + hGrad[2];
            Int3 vGrad = Hlsl.Abs(slice[0] - slice[2]);
            int vGradMagnitude = vGrad[0] + vGrad[1] + vGrad[2];
            inflate[ThreadIds.XYZ] = Hlsl.BoolToInt(hGradMagnitude == vGradMagnitude && hGradMagnitude != 0) & slice[1][1];
        }
    }
    
    [ThreadGroupSize(DefaultThreadGroupSizes.XY)]
    [GeneratedComputeShaderDescriptor]
    [SupportedOSPlatform("windows")]
    internal readonly partial struct DeflateTextureShader(ReadWriteTexture2D<int> corners, ReadWriteTexture3D<int> inflated) : IComputeShader
    {
        public void Execute()
        {
            int sum = 0;
            for (int i = 0; i < inflated.Depth; i++)
            {
                sum += inflated[ThreadIds.X * 2,     ThreadIds.Y * 2,    i];
                sum += inflated[ThreadIds.X * 2 + 1, ThreadIds.Y * 2,    i];
                sum += inflated[ThreadIds.X * 2,     ThreadIds.Y * 2 + 1,i];
                sum += inflated[ThreadIds.X * 2 + 1, ThreadIds.Y * 2 + 1,i];
            }
            corners[ThreadIds.XY] = sum;
        }
    }

    [SupportedOSPlatform("Windows")]
    public int[,] GetCornersCuda(int[,] ids, int idCount)
    {
        int cRows = ids.GetLength(0);
        int cCols = ids.GetLength(1);
        GraphicsDevice g = GraphicsDevice.GetDefault();
        ReadOnlyTexture2D<int> idTexture = g.AllocateReadOnlyTexture2D(ids);
        ReadWriteTexture3D<int> inflateTexture = g.AllocateReadWriteTexture3D<int>(cRows * 2, cCols * 2, idCount);

        InflateTextureShader inflateShader = new(idTexture, inflateTexture);
        g.For(cRows * 2, cCols * 2, idCount, inflateShader);
        CornerDetectShader cornerShader = new(inflateTexture);
        g.For(cRows * 2, cCols * 2, idCount, cornerShader);
        ReadWriteTexture2D<int> cornerCounts = g.AllocateReadWriteTexture2D<int>(cRows, cCols);
        DeflateTextureShader deflateShader = new(cornerCounts, inflateTexture);
        g.For(cRows, cCols, deflateShader);
        return cornerCounts.ToArray();
    }

    public int[,] GetCornersCpu(int[,] ids, int idCount)
    {
        int cRows = ids.GetLength(0);
        int cCols = ids.GetLength(1);
        int[,,] inflated = new int[idCount, cRows * 2, cCols * 2];
        for (int r = 0; r < cRows; r++)
        for (int c = 0; c < cCols; c++)
        {
            int id = ids[r, c];
            inflated[id, r * 2, c * 2] = 1;
            inflated[id, r * 2 + 1, c * 2] = 1;
            inflated[id, r * 2, c * 2 + 1] = 1;
            inflated[id, r * 2 + 1, c * 2 + 1] = 1;
        }
        
        int[,,] result = new int[idCount, cRows * 2, cCols * 2];
        int[,] slice = new int[3, 3];
        for (int i = 0; i < idCount; i++)
        for (int r = 0; r < cRows * 2; r++)
        for (int c = 0; c < cCols * 2; c++)
        {
            Slice(i, r, c);
            int hGrad = (slice[0, 0] - slice[0, 2]).Square() + (slice[1, 0] - slice[1, 2]).Square() + (slice[2, 0] - slice[2, 2]).Square();
            int vGrad = (slice[0, 0] - slice[2, 0]).Square() + (slice[0, 1] - slice[2, 1]).Square() + (slice[0, 2] - slice[2, 2]).Square();
            result[i, r, c] = hGrad == vGrad && hGrad != 0 ? 1 & slice[1,1] : 0;
        }

        void Slice(int i, int r, int c)
        {
            slice[0, 0] = inflated.Get(i, r - 1, c - 1);
            slice[0, 1] = inflated.Get(i, r - 1, c);
            slice[0, 2] = inflated.Get(i, r - 1, c + 1);
            slice[1, 0] = inflated.Get(i, r, c - 1);
            slice[1, 1] = inflated.Get(i, r, c);
            slice[1, 2] = inflated.Get(i, r, c + 1);
            slice[2, 0] = inflated.Get(i, r + 1, c - 1);
            slice[2, 1] = inflated.Get(i, r + 1, c);
            slice[2, 2] = inflated.Get(i, r + 1, c + 1);
        }

        int[,] deflated = new int[cRows, cCols];
        for (int i = 0; i < idCount; i++)
        for (int r = 0; r < cRows; r++)
        for (int c = 0; c < cCols; c++)
        {
            deflated[r,c] += result[i, r * 2, c * 2] +
                result[i, r * 2 + 1, c * 2] +
                result[i, r * 2, c * 2 + 1] +
                result[i, r * 2 + 1, c * 2 + 1];
        }

        return deflated;
    }
}