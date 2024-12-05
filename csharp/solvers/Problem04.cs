using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChadNedzlek.AdventOfCode.Library;

namespace ChadNedzlek.AdventOfCode.Y2024.CSharp
{
    public class Problem04 : AsyncProblemBase
    {
        protected override async Task ExecuteCoreAsync(string[] data)
        {
            int[] four = [0, 1, 2, 3];
            int[,] halfMases = data.Select2D(_ => 0);
            var xmases = data.AsEnumerableWithIndex()
                .Select((_, r, c) => new GPoint2<int>(r, c))
                .Sum(p => Helpers.EightDirections
                    .Sum(d => new string(four.Select(i => data.Get(p + d * i)).ToArray()) is "XMAS" ? 1 : 0));
            
            for (int r = 0; r < data.Length; r++)
            {
                for (int c = 0; c < data[r].Length; c++)
                {
                    GPoint2<int> x = (r, c);
                    
                    foreach(var p in  Helpers.DiagonalDirections)
                    {
                        var str = new string(Enumerable.Range(0, 3).Select(m => data.Get(x + m * p)).ToArray());
                        if (str is "MAS")
                        {
                            (x + p).Do(a => halfMases[a.Row, a.Col]++);
                            Helpers.VerboseLine($"Found half-MAS at {x + p} in direction {p}");
                        }
                    }
                }
            }
            
            Console.WriteLine($"Found {xmases} XMAS's");
            Console.WriteLine($"Found {halfMases.AsEnumerable().Count(a => a == 2)} X-MAS's");
        }
    }
}