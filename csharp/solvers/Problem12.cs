using System;
using System.Collections.Generic;
using System.Linq;
using ChadNedzlek.AdventOfCode.Library;
using TorchSharp;

namespace ChadNedzlek.AdventOfCode.Y2024.CSharp;

public class Problem12 : SyncProblemBase
{
    protected override void ExecuteCore(string[] data)
    {
        int[,] ids = data.Select2D(_ => 0);

        int count = 0;

        int cRows = data.Length;
        int cCols = data[0].Length;
        for (int r = 0; r < cRows; r++)
        {
            for (int c = 0; c < cCols; c++)
            {
                if (ids[r, c] == 0)
                {
                    MarkRegion((r, c), ++count, data[r][c]);
                }
            }
        }

        Dictionary<int, int> area = [];
        Dictionary<int, int> border = [];
        Dictionary<int, int> corners = [];
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

        Dictionary<int, int> cornerDetection = [];

        for (int r = 0; r <= cRows; r++)
        for (int c = 0; c <= cCols; c++)
        {
            var p = new GPoint2<int>(r, c);
            cornerDetection.Clear();
            cornerDetection.Increment(ids.Get(p - (1,1)));
            cornerDetection.Increment(ids.Get(p - (0,1)));
            cornerDetection.Increment(ids.Get(p - (1,0)));
            cornerDetection.Increment(ids.Get(p));
            int crisscrossAppleSauce = ids.Get(p) == ids.Get(p - (1, 1)) || ids.Get(p - (0, 1)) == ids.Get(p - (1, 0)) ? 2 : 0;

            foreach ((int id, int overlaps) in cornerDetection)
            {
                if (overlaps == 2)
                {
                    corners.Increment(id, crisscrossAppleSauce);
                }

                if (overlaps % 2 == 1)
                {
                    corners.Increment(id);
                }
            }
        }

        int[] allIds = area.Keys.ToArray();
        foreach (var id in allIds)
        {
            Helpers.VerboseLine($"A region of {letters[id]} plants with area: {area[id]}, border: {border[id]}, sides(corners): {corners[id]}");
        }
        Console.WriteLine($"Part 1 = {allIds.Sum(i => area[i] * border[i])}");
        Console.WriteLine($"Part 2 = {allIds.Sum(i => area[i] * corners[i])}");

        void MarkRegion(GPoint2<int> p, int id, char c)
        {
            if (ids.Get(p, -1) != 0 || data.Get(p) != c)
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
}