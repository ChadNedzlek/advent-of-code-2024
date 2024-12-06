using System;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using ChadNedzlek.AdventOfCode.Library;

namespace ChadNedzlek.AdventOfCode.Y2024.CSharp
{
    public class Problem06 : DualAsyncProblemBase
    {
        protected override async Task ExecutePart1Async(string[] data)
        {
            bool[,] visited = data.Select2D(_ => false);
            GPoint2<int> gaurd = data.AsEnumerableWithIndex().First(x => x.value == '^').Into(x => new GPoint2<int>(x.index0, x.index1));
            GPoint2<int> dir = (-1, 0);
            while (visited.IsInRange(gaurd))
            {
                visited.TrySet(gaurd, true);
                while (data.Get(gaurd + dir) == '#')
                {
                    dir = (dir.Col, -dir.Row);
                }

                gaurd += dir;
            }
            Console.WriteLine($"Visited {visited.AsEnumerable().Count(x => x)}");
        }

        protected override async Task ExecutePart2Async(string[] data)
        {
            char[,] baseMap = data.Select2D(x => x);
            GPoint2<int> baseGaurd = data.AsEnumerableWithIndex().First(x => x.value == '^').Into(x => new GPoint2<int>(x.index0, x.index1));

            var loopable = baseMap.AsEnumerableWithIndex().Count(x => x.value == '.' && IsLoop(baseMap.WithValueSet((x.index0, x.index1), '#')));
            
            Console.WriteLine($"Made {loopable} loops");

            bool IsLoop(char[,] map)
            {
                GPoint2<int> gaurd = baseGaurd;
                GPoint2<int>[,] visited = map.Select2D((_, _, _) => new GPoint2<int>(0,0));
                GPoint2<int> dir = (-1, 0);
                while (visited.IsInRange(gaurd))
                {
                    if (visited.Get(gaurd) == dir)
                    {
                        return true;
                    }

                    visited.TrySet(gaurd, dir);
                    
                    while (map.Get(gaurd + dir) == '#')
                    {
                        dir = (dir.Col, -dir.Row);
                    }

                    gaurd += dir;
                }

                return false;
            }
        }
    }
}