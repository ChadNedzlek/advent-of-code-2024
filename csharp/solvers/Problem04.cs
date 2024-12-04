using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChadNedzlek.AdventOfCode.Library;

namespace ChadNedzlek.AdventOfCode.Y2023.CSharp.solvers
{
    public class Problem04 : AsyncProblemBase
    {
        protected override async Task ExecuteCoreAsync(string[] data)
        {
            int xmases = 0;
            int pattern = 0;
            Infinite2I<int> mas = new();
            for (int r = 0; r < data.Length; r++)
            {
                for (int c = 0; c < data[r].Length; c++)
                {
                    GPoint2I x = (r, c);
                    for (int i = 0; i < 8; i++)
                    {
                        var p = Helpers.EightDirections[i];
                        var str = new string(Enumerable.Range(0, 4).Select(m => x + m * p).Select(d => data.Get(d)).ToArray());
                        if (str is "XMAS")
                        {
                            Helpers.VerboseLine($"Found XMAS at {r},{c} in direction {i}");
                            xmases++;
                        }
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        var p = Helpers.DiagonalDirections[i];
                        var str = new string(Enumerable.Range(0, 3).Select(m => x + m * p).Select(d => data.Get(d)).ToArray());
                        if (str is "MAS")
                        {
                            var a = x + p;
                            mas[a.Row, a.Col]++;
                            Helpers.VerboseLine($"Found half-MAS at {a} in direction {i}");
                        }
                    }
                }
            }
            
            Console.WriteLine($"Found {xmases} XMAS's");
            Console.WriteLine($"Found {mas.Count(a => a == 2)} X-MAS's");
        }
    }
}