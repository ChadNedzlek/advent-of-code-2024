using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ChadNedzlek.AdventOfCode.Library;

namespace ChadNedzlek.AdventOfCode.Y2024.CSharp
{
    public class Problem07 : DualProblemBase
    {
        public Problem07(string executionMode) : base(executionMode)
        {
        }

        protected override void ExecutePart1(string[] data)
        {
            long reached = 0;
            foreach (var line in data)
            {
                Match m = Regex.Match(line, @"^(\d+):(\s*\d+)*$");
                if (!m.Success) continue;
                long total = long.Parse(m.Groups[1].Value);
                long[] parts = m.Groups[2].Captures.Select(c => long.Parse(c.Value)).ToArray();
                for (long op = 0; op < 1 << (parts.Length - 1); op++)
                {
                    long v = parts[0];
                    for (int i = 1; i < parts.Length; i++)
                    {
                        v = ((op >> (i-1)) & 1L) == 1 ? v * parts[i] : v + parts[i];

                        if (v > total)
                        {
                            break;
                        }
                    }

                    if (v == total)
                    {
                        Helpers.VerboseLine($"Total {total} reached with op {op:B}");
                        reached += total;
                        break;
                    }
                }
            }
            Console.WriteLine($"Reached {reached} calibration");
        }

        protected override void  ExecutePart2(string[] data)
        {
            long reached = 0;
            foreach (var line in data)
            {
                Match m = Regex.Match(line, @"^(\d+):(\s*\d+)*$");
                if (!m.Success) continue;
                long total = long.Parse(m.Groups[1].Value);
                string[] strs = m.Groups[2].Captures.Select(c => c.Value.TrimStart()).ToArray();
                long[] parts = strs.Select(long.Parse).ToArray();
                for (long op = 0; op < 3.Pow(parts.Length - 1); op++)
                {
                    long v = parts[0];
                    for (int i = 1; i < parts.Length; i++)
                    {
                        v = (op / 3.Pow(i - 1) % 3) switch
                        {
                            0 => v * parts[i],
                            1 => v + parts[i],
                            2 => long.Parse(v + strs[i]),
                            _ => throw new ArgumentException(),
                        };

                        if (v > total)
                        {
                            break;
                        }
                    }

                    if (v == total)
                    {
                        Helpers.VerboseLine($"Total {total} reached with op {op:B}");
                        reached += total;
                        break;
                    }
                }
            }
            Console.WriteLine($"Reached {reached} calibration");
        }
    }
}