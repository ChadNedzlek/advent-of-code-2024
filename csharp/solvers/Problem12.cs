using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using ChadNedzlek.AdventOfCode.Library;
using ComputeSharp;
using TorchSharp;

namespace ChadNedzlek.AdventOfCode.Y2024.CSharp;

public class Problem12 : SyncProblemBase
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
        if (OperatingSystem.IsWindows() && false)
        {
            corners = GetCornersCuda(ids, idCount);
        }
        else
        {
            corners = GetCornersCpu(ids, idCount);
        }


        Dictionary<int, int> cornerCounts = [];

        for (int r = 0; r < cRows; r++)
        for (int c = 0; c < cCols; c++)
        {
            cornerCounts.Increment(ids[r, c], corners[r, c]);
        }

        int[] allIds = area.Keys.ToArray();
        foreach (var id in allIds)
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
                foreach (var neighbor in Helpers.OrthogonalDirections)
                {
                    MarkRegion(p + neighbor, id, c);
                }
            }
        }
    }

    [SupportedOSPlatform("Windows")]
    public int[,] GetCornersCuda(int[,] ids, int idCount)
    {
        int cRows = ids.GetLength(0);
        int cCols = ids.GetLength(1);
        var g = GraphicsDevice.GetDefault();
        ReadOnlyTexture2D<int> inflatedTexture = g.AllocateReadOnlyTexture2D(ids);
        ReadWriteTexture3D<int> inflatedTexture3D = g.AllocateReadWriteTexture3D<int>(cRows * 2, cCols * 2, idCount);
        throw new NotSupportedException();
    }

    public int[,] GetCornersCpu(int[,] ids, int idCount)
    {
        int cRows = ids.GetLength(0);
        int cCols = ids.GetLength(1);
        var inflated = new int[idCount, cRows * 2, cCols * 2];
        for (int r = 0; r < cRows; r++)
        for (int c = 0; c < cCols; c++)
        {
            int id = ids[r, c];
            inflated[id, r * 2, c * 2] = 1;
            inflated[id, r * 2 + 1, c * 2] = 1;
            inflated[id, r * 2, c * 2 + 1] = 1;
            inflated[id, r * 2 + 1, c * 2 + 1] = 1;
        }
        
        var result = new int[idCount, cRows * 2, cCols * 2];
        for (int i = 0; i < idCount; i++)
        for (int r = 0; r < cRows; r++)
        for (int c = 0; c < cCols; c++)
        {
            int sum = (
                -1 * inflated.Get(i, r - 1, c) +
                1 * inflated.Get(i, r + 1, c) +
                -3 * inflated.Get(i, r, c - 1) +
                3 * inflated.Get(i, r, c + 1)
            );

            result[i, r, c] = sum & 1;
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