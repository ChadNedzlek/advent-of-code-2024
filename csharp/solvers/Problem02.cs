using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;
using ChadNedzlek.AdventOfCode.Library;

namespace ChadNedzlek.AdventOfCode.Y2023.CSharp.solvers
{
    public class Problem02 : AsyncProblemBase
    {
        protected override async Task ExecuteCoreAsync(string[] data)
        {
            var part1 = data.Select(d => d.Split(' ').Select(int.Parse).ToArray()).ToList();
            int safe = 0;
            foreach (int[] p in part1)
            {
                int min = 0, max = 0, maxDelta = 0, minDelta = 3;
                for (int i = 0; i < p.Length - 1; i++)
                {
                    int n = p[i];
                    int d = p[i + 1] - n;
                    min = int.Min(min, d);
                    max = int.Max(max, d);
                    maxDelta = int.Max(Math.Abs(d) , maxDelta);
                    minDelta = int.Min(Math.Abs(d) , minDelta);
                }

                if ((min == 0 || max == 0) && maxDelta <= 3 && minDelta >= 1)
                {
                    safe++;
                }
                else
                {
                }
            }
            Console.WriteLine($"Safe: {safe}");
            int safe2 = 0;
            foreach (int[] x in part1)
            {
                ImmutableList<int> imm = x.ToImmutableList();
                for (int r = 0; r < imm.Count; r++)
                {
                    var p = imm.RemoveAt(r);
                    int min = 0, max = 0, maxDelta = 0, minDelta = 3;
                    for (int i = 0; i < p.Count - 1; i++)
                    {
                        int n = p[i];
                        int d = p[i + 1] - n;
                        min = int.Min(min, d);
                        max = int.Max(max, d);
                        maxDelta = int.Max(Math.Abs(d), maxDelta);
                        minDelta = int.Min(Math.Abs(d), minDelta);
                    }

                    if ((min == 0 || max == 0) && maxDelta <= 3 && minDelta >= 1)
                    {
                        safe2++;
                        break;
                    }
                }
            }
            Console.WriteLine($"Safish: {safe2}");
            JeremyVersion();
            void JeremyVersion()
            {
                Problem2();
                void Problem2()
                {
                    long ret = 0;

                    foreach (string s in data)
                    {
                        List<long> allValues = s.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(long.Parse).ToList();
                        bool safe = IsSafe(allValues);

                        if (!safe)
                        {
                            for (int i = 0; i < allValues.Count; i++)
                            {
                                List<long> curValues = new List<long>(allValues);
                                curValues.RemoveAt(i);

                                if (IsSafe(curValues))
                                {
                                    Console.WriteLine($"Made {string.Join(",", allValues)} safe by removing index {i}: {string.Join(",", curValues)}");
                                    safe = true;
                                    break;
                                }
                            }
                        }

                        if (safe)
                        {
                            ret++;
                        }
                        else
                        {
                            Console.WriteLine($"UNSAFE: {string.Join(",", allValues)}");
                        }
                    }

                    Console.WriteLine(ret);
                }
                static bool IsSafe(List<long> values)
                {
                    long sign = long.Sign(values[1] - values[0]);

                    for (int i = 1; i < values.Count; i++)
                    {
                        long prev = values[i - 1];
                        long cur = values[i];
                        long delta = cur - prev;

                        if (long.Sign(delta) != sign || long.Abs(delta) > 3)
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }
        }
    }
}