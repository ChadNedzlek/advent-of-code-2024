using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChadNedzlek.AdventOfCode.Library;

namespace ChadNedzlek.AdventOfCode.Y2024.CSharp
{
    public class Problem06 : DualProblemBase
    {
        private static bool[,] GetVisited(string[] data)
        {
            bool[,] visited = data.Select2D(_ => false);
            GPoint2<int> guard = data.AsEnumerableWithIndex().First(x => x.value == '^').Into(x => new GPoint2<int>(x.index0, x.index1));
            GPoint2<int> dir = (-1, 0);
            while (visited.IsInRange(guard))
            {
                visited.TrySet(guard, true);
                while (data.Get(guard + dir) == '#')
                {
                    dir = (dir.Col, -dir.Row);
                }

                guard += dir;
            }

            return visited;
        }

        protected override void ExecutePart1(string[] data)
        {
            bool[,] visited = GetVisited(data);
            Console.WriteLine($"Visited {visited.AsEnumerable().Count(x => x)}");
        }

        protected override void  ExecutePart2(string[] data)
        {
            bool[,] originalPath = GetVisited(data);
            char[,] baseMap = data.Select2D(x => x);
            GPoint2<int> baseGuard = data.AsEnumerableWithIndex().First(x => x.value == '^').Into(x => new GPoint2<int>(x.index0, x.index1));

            int count = 0;
            Parallel.ForEach(baseMap.AsEnumerableWithIndex(), x => IsLoop(baseMap, baseGuard, (x.index0, x.index1), ref count, originalPath));
            
            Console.WriteLine($"Made {count} loops");

            static void IsLoop(char[,] map, GPoint2<int> guard, GPoint2<int> extraObstacle, ref int count, bool[,] validNewObstacles)
            {
                if (map.Get(extraObstacle) != '.' || !validNewObstacles.Get(extraObstacle)) return;

                GPoint2<int>[,] visited = new GPoint2<int>[map.GetLength(0), map.GetLength(1)];
                GPoint2<int> dir = (-1, 0);
                while (visited.IsInRange(guard))
                {
                    if (visited.Get(guard) == dir)
                    {
                        Interlocked.Increment(ref count);
                        return;
                    }

                    visited.TrySet(guard, dir);
                    
                    while ((guard + dir).Into(l => extraObstacle == l || map.Get(l) == '#'))
                    {
                        dir = (dir.Col, -dir.Row);
                    }

                    guard += dir;
                }

                return;
            }
        }
    }
}