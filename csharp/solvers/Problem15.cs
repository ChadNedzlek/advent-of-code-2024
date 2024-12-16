using System;
using System.Linq;
using ChadNedzlek.AdventOfCode.Library;

namespace ChadNedzlek.AdventOfCode.Y2024.CSharp;

public class Problem15 : SyncProblemBase
{
    protected override void ExecuteCore(string[] data)
    {
        var moves = string.Join("", data.SkipWhile(x => !string.IsNullOrWhiteSpace(x)));
        
        var map = data.TakeWhile(x => !string.IsNullOrWhiteSpace(x)).ToArray();
        GPoint2<int> location = data.AsEnumerableWithPoint().FirstOrDefault(x => x.value == '@').point;
        long value = RunMap(map, location);
        Console.WriteLine($"First Part: {value}");

        var expandedMap = data.TakeWhile(x => !string.IsNullOrWhiteSpace(x))
            .Select(s => s.Replace(".", "..").Replace("#", "##").Replace("O", "[]").Replace("@", "@."))
            .ToArray();
        GPoint2<int> expandedLocation = data.AsEnumerableWithPoint().FirstOrDefault(x => x.value == '@').point.Into(p => (p.Row, p.Col * 2));
        long value2 = RunMap(expandedMap, expandedLocation);
        Console.WriteLine($"Second Part: {value2}");

        long RunMap(string[] map, GPoint2<int> loc)
        {
            // map[location.Row] = map[location.Row].Replace('@', '.');

            if (Helpers.IncludeVerboseOutput)
            {
                for (int r = 0; r < map.Length; r++)
                {
                    for (int c = 0; c < map[r].Length; c++)
                    {
                        // if (location == (r, c))
                        // {
                        //     Console.Write('@');
                        // }
                        // else
                        {
                            Console.Write(map[r][c]);
                        }
                    }

                    Console.WriteLine();
                }

                Console.WriteLine();
            }

            foreach (var ins in moves)
            {
                GPoint2<int> dir = ins switch
                {
                    '^' => (-1, 0),
                    'v' => (1, 0),
                    '<' => (0, -1),
                    '>' => (0, 1),
                };

                if (TryMove(loc, dir))
                {
                    loc += dir;
                }

                if (Helpers.IncludeVerboseOutput)
                {
                    for (int r = 0; r < map.Length; r++)
                    {
                        for (int c = 0; c < map[r].Length; c++)
                        {
                            // if (location == (r, c))
                            // {
                            //     Console.Write('@');
                            // }
                            // else
                            {
                                Console.Write(map[r][c]);
                            }
                        }

                        Console.WriteLine();
                    }

                    Console.WriteLine();
                }

            }

            long l = 0;
            for (int r = 0; r < map.Length; r++)
            for (int c = 0; c < map[r].Length; c++)
            {
                if (map[r][c] is 'O' or '[')
                {
                    l += 100 * r + c;
                }
            }

            return l;

            bool TryMove(GPoint2<int> loc, GPoint2<int> dir, bool dryRun = false, bool skipHalf = false)
            {
                switch (map.Get(loc))
                {
                    case '#': return false;
                    case 'O':
                    case '@':
                    {
                        if (TryMove(loc + dir, dir, dryRun, skipHalf))
                        {
                            if (!dryRun)
                            {
                                map.TrySet(loc + dir, map.Get(loc));
                                map.TrySet(loc, '.');
                            }

                            return true;
                        }

                        return false;
                    }
                    case ']':
                        if ((dir == (0,-1) || TryMove(loc + dir, dir, true)) && (skipHalf || TryMove(loc - (0, 1), dir, true, true)))
                        {
                            if (!dryRun)
                            {
                                TryMove(loc + dir, dir, skipHalf: dir == (0,-1));
                                if (!skipHalf && dir != (0,-1)) TryMove(loc - (0, 1), dir, skipHalf: true);
                                map.TrySet(loc + dir, map.Get(loc));
                                map.TrySet(loc, '.');
                            }

                            return true;
                        }

                        return false;
                    case '[':
                        if ((dir == (0,1) || TryMove(loc + dir, dir, true)) && (skipHalf || TryMove(loc + (0, 1), dir, true, true)))
                        {
                            if (!dryRun)
                            {
                                TryMove(loc + dir, dir, skipHalf: dir == (0,1));
                                if (!skipHalf && dir != (0,1)) TryMove(loc + (0, 1), dir, skipHalf: true);
                                map.TrySet(loc + dir, map.Get(loc));
                                map.TrySet(loc, '.');
                            }

                            return true;
                        }

                        return false;

                    default: return true;
                }
            }
        }
    }
}